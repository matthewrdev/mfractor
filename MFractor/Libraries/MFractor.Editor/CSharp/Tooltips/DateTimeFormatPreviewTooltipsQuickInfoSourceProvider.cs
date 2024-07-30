using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.IOC;
using MFractor.Licensing;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.CSharp.Tooltips
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("MFractor.CSharp.DateTimeFormatPreview")]
    [ContentType(ContentTypes.CSharp)]
    [Order(After = "default")]
    sealed class DateTimeFormatPreviewTooltipsQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [ImportingConstructor]
        public DateTimeFormatPreviewTooltipsQuickInfoSourceProvider(ILicensingService licensingService,
                                                    ITextDocumentFactoryService textDocumentFactory)
        {
            this.licensingService = licensingService;
            this.textDocumentFactory = textDocumentFactory;
        }

        readonly ILicensingService licensingService;

        readonly ITextDocumentFactoryService textDocumentFactory;

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            if (!licensingService.IsPaid)
            {
                return default;
            }

            if (!textDocumentFactory.TryGetTextDocument(textBuffer, out var textDocument))
            {
                return null;
            }

            var project = TextBufferHelper.GetCompilationProject(textBuffer);
            if (project == null)
            {
                return null;
            }

            // This ensures only one instance per textbuffer is created
            return textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                var source = Resolver.Resolve<DateTimeFormatPreviewTooltipsQuickInfoSource>();
                source.Initialise(textDocument.FilePath, project.Id);

                return source;
            });
        }
    }
}
