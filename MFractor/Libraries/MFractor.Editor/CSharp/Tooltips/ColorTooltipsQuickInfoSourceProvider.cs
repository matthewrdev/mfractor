using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Licensing;
using MFractor.Workspace;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.CSharp.Tooltips
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("MFractor.CSharp.ColorTooltips")]
    [ContentType(ContentTypes.CSharp)]
    [Order(After = "default")]
    sealed class ColorTooltipsQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [ImportingConstructor]
        public ColorTooltipsQuickInfoSourceProvider(ILicensingService licensingService,
                                                    IAnalyticsService analyticsService,
                                                    ITextDocumentFactoryService textDocumentFactory,
                                                    IWorkspaceService workspaceService)
        {
            this.licensingService = licensingService;
            this.analyticsService = analyticsService;
            this.textDocumentFactory = textDocumentFactory;
            this.workspaceService = workspaceService;
        }

        readonly ILicensingService licensingService;

        readonly IAnalyticsService analyticsService;

        readonly ITextDocumentFactoryService textDocumentFactory;

        readonly IWorkspaceService workspaceService;

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
                var source = new ColorTooltipsQuickInfoSource(analyticsService, workspaceService, textDocument.FilePath, project.Id);

                return source;
            });
        }
    }
}
