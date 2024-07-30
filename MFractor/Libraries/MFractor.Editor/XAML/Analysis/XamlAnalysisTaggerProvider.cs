using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Editor.XAML.Services;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.Analysis
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ErrorTag))]
    [ContentType(ContentTypes.Xaml)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    class XamlAnalysisTaggerProvider : IViewTaggerProvider
    {
        readonly Logging.ILogger log = Logging.Logger.Create();


        readonly Dictionary<ITextView, XamlAnalysisTagger> cache = new Dictionary<ITextView, XamlAnalysisTagger>();

        readonly Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService;
        public IXmlSyntaxTreeService XmlSyntaxTreeService => xmlSyntaxTreeService.Value;

        readonly Lazy<ITextDocumentFactoryService> textDocumentFactory;
        public ITextDocumentFactoryService TextDocumentFactory => textDocumentFactory.Value;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        [ImportingConstructor]
        internal XamlAnalysisTaggerProvider(Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService,
                                            Lazy<ITextDocumentFactoryService> textDocumentFactory,
                                            Lazy<ILicensingService> licensingService)
        {
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (!TextDocumentFactory.TryGetTextDocument(buffer, out var textDocument))
            {
                return null;
            }

            if (!LicensingService.IsPaid)
            {
                return null;
            }

            XamlAnalysisTagger tagger = null;

            if (cache.TryGetValue(textView, out var cachedTagger))
            {
                return cachedTagger as ITagger<T>;
            }
            else
            {
                if ((buffer == textView.TextBuffer) && (typeof(T) == typeof(IErrorTag)))
                {
                    try
                    {
                        tagger = Resolver.Resolve<XamlAnalysisTagger>();
                        tagger.Initialise(textView, textDocument.FilePath);

                        cache.Add(textView, tagger);

                        BindTextViewLifecycleEvents(textDocument.FilePath, textView);
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }
            }

            return tagger as ITagger<T>;
        }

        void BindTextViewLifecycleEvents(string filePath, ITextView textView)
        {
            void textViewClosed(object sender, EventArgs e)
            {
                if (cache.TryGetValue(textView, out var tagger))
                {
                    tagger.Dispose();
                }
                cache.Remove(textView);
                textView.Closed -= textViewClosed;
            }

            textView.Closed += textViewClosed;

            (XmlSyntaxTreeService as XmlSyntaxTreeService).BindTextView(filePath, textView);
        }
    }
}