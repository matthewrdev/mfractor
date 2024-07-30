using System.ComponentModel.Composition;
using MFractor.IOC;
using MFractor.Licensing;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.Tooltips
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("MFractor.XamlTooltips")]
    [ContentType(ContentTypes.Xaml)]
    [Order(After = "default")]
    sealed class XamlTooltipsQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [ImportingConstructor]
        public XamlTooltipsQuickInfoSourceProvider(ITextDocumentFactoryService textDocumentFactory,
                                                   ILicensingService licensingService)
        {
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
        }

        readonly ITextDocumentFactoryService textDocumentFactory;
        readonly ILicensingService licensingService;

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
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
                var source = Resolver.Resolve<XamlTooltipsQuickInfoSource>();
                source.SetTextBuffer(textBuffer, textDocument.FilePath);
                return source;
            });
        }
    }
}
