using System;using System.ComponentModel.Composition;
using MFractor.IOC;
using MFractor.Licensing;
using Microsoft.VisualStudio.Language.Intellisense;using Microsoft.VisualStudio.Text;using Microsoft.VisualStudio.Text.Editor;using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.CodeActions
{

#if VS_MAC
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("MFractor.Xaml.CodeActions")]
    [ContentType(ContentTypes.Xaml)]
    class XamlCodeActionsSourceProvider : ISuggestedActionsSourceProvider  {
        [ImportingConstructor]
        public XamlCodeActionsSourceProvider(ITextDocumentFactoryService textDocumentFactory,
                                             ILicensingService licensingService)
        {
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
        }

        readonly ITextDocumentFactoryService textDocumentFactory;
        readonly ILicensingService licensingService;
        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)        {
            if (!textDocumentFactory.TryGetTextDocument(textBuffer, out var textDocument))
            {
                return null;
            }

            if (!licensingService.IsPaid)
            {
                return null;
            }

            // This ensures only one instance per textbuffer is created
            return textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                var source = Resolver.Resolve<XamlCodeActionsSource>();
                source.Initialise(textDocument.FilePath, textView);

                void textViewClosed(object sender, EventArgs args)
                {
                    source.Dispose();
                    textView.Closed -= textViewClosed;
                }

                textView.Closed += textViewClosed;
                return source;
            });        }
    }
#endif}