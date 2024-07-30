using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Licensing;
using MFractor.Xml;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using MFractor.IOC;

namespace MFractor.Editor.XAML.Completion
{
    using MFractor.Ide;
#if VS_MAC
    using Microsoft.VisualStudio.Utilities;
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name("MFractor Xaml Code Completion")]
    [ContentType(ContentTypes.Xaml)]
    [Order(After = "default")]
#endif
    class XamlAsyncCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        [ImportingConstructor]
        public XamlAsyncCompletionSourceProvider(IIdeFeatureSettings featureSettings,
                                                 ITextDocumentFactoryService textDocumentFactory,
                                                 ILicensingService licensingService)
        {
            this.featureSettings = featureSettings;
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
        }

        readonly IDictionary<ITextView, IAsyncCompletionSource> cache = new Dictionary<ITextView, IAsyncCompletionSource>();

        readonly IIdeFeatureSettings featureSettings;

        readonly ITextDocumentFactoryService textDocumentFactory;

        readonly ILicensingService licensingService;

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            if (!featureSettings.UseXAMLIntelliSense)
            {
                return null;
            }

            if (!textDocumentFactory.TryGetTextDocument(textView.TextBuffer, out var textDocument))
            {
                return null;
            }

            if (!licensingService.IsPaid)
            {
                return null;
            }

            var project = TextBufferHelper.GetCompilationProject(textView.TextBuffer);
            if (project == null)
            {
                return null;
            }

            if (cache.TryGetValue(textView, out var itemSource))
            {
                return itemSource;
            }

            var source = Resolver.Resolve<XamlAsyncCompletionSource>();
            source.Initialise(textDocument.FilePath, project.Id);

            cache.Add(textView, source);

            BindTextViewLifecycleEvents(textView);

            return source;
        }

        void BindTextViewLifecycleEvents(ITextView textView)
        {
            void textViewClosed(object sender, EventArgs e)
            {
                cache.Remove(textView); // clean up memory as files are closed
                textView.Closed -= textViewClosed;
            }

            textView.Closed += textViewClosed;
        }
    }
}
