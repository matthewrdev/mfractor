using System.ComponentModel.Composition;
using MFractor.IOC;
using MFractor.Licensing;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.Analysis
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("MFractor.XamlAnalysisTooltips")]
    [ContentType(ContentTypes.Xaml)]
    [Order(Before = "MFractor.XamlTooltips")]
    sealed class XamlAnalysisQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [ImportingConstructor]
        public XamlAnalysisQuickInfoSourceProvider(ITextDocumentFactoryService textDocumentFactory,
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
                var source = Resolver.Resolve<XamlAnalysisQuickInfoSource>();
                source.SetFile(textDocument.FilePath);

                return source;
            });
        }
    }
}
