using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Code;
using MFractor.Editor.Utilities;
using MFractor.Ide;
using MFractor.Images;
using MFractor.Licensing;
using MFractor.Workspace;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.CSharp.Tooltips
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("MFractor.CSharp.ImageTooltips")]
    [ContentType(ContentTypes.CSharp)]
    [Order(After = "default")]
    sealed class ImageTooltipsQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [ImportingConstructor]
        public ImageTooltipsQuickInfoSourceProvider(ILicensingService licensingService,
                                                    IImageAssetService imageAssetService,
                                                    IFeatureContextFactoryRepository featureContextFactories,
                                                    IAnalyticsService analyticsService,
                                                    ITextDocumentFactoryService textDocumentFactory,
                                                    IWorkspaceService workspaceService,
                                                    IIdeImageManager ideImageManager)
        {
            this.licensingService = licensingService;
            this.imageAssetService = imageAssetService;
            this.featureContextFactories = featureContextFactories;
            this.analyticsService = analyticsService;
            this.textDocumentFactory = textDocumentFactory;
            this.workspaceService = workspaceService;
            this.ideImageManager = ideImageManager;
        }

        readonly ILicensingService licensingService;

        readonly IImageAssetService imageAssetService;

        readonly IFeatureContextFactoryRepository featureContextFactories;

        readonly IAnalyticsService analyticsService;

        readonly ITextDocumentFactoryService textDocumentFactory;

        readonly IWorkspaceService workspaceService;
        readonly IIdeImageManager ideImageManager;

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
                var source = new ImageTooltipsQuickInfoSource(imageAssetService, analyticsService, featureContextFactories, workspaceService, ideImageManager, textDocument.FilePath, project.Id);

                return source;
            });
        }
    }
}
