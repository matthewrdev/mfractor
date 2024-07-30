using System;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Android.Helpers;
using MFractor.Code;
using MFractor.Ide;
using MFractor.Images;
using MFractor.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace MFractor.Editor.CSharp.Tooltips
{
    class ImageTooltipsQuickInfoSource : IAsyncQuickInfoSource
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly IImageAssetService imageAssetService;
        readonly IAnalyticsService analyticsService;
        readonly IFeatureContextFactoryRepository featureContextFactories;
        readonly IWorkspaceService workspaceService;
        readonly IIdeImageManager ideImageManager;

        readonly string filePath;
        readonly ProjectId projectId;

        public ImageTooltipsQuickInfoSource(IImageAssetService imageAssetService,
                                            IAnalyticsService analyticsService,
                                            IFeatureContextFactoryRepository featureContextFactories,
                                            IWorkspaceService workspaceService,
                                            IIdeImageManager ideImageManager,
                                            string filePath,
                                            ProjectId projectId)
        {
            this.imageAssetService = imageAssetService;
            this.analyticsService = analyticsService;
            this.featureContextFactories = featureContextFactories;
            this.workspaceService = workspaceService;
            this.ideImageManager = ideImageManager;
            this.filePath = filePath;
            this.projectId = projectId;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session,
                                                         CancellationToken cancellationToken)
        {
            try
            {
                var project = workspaceService.CurrentWorkspace.CurrentSolution.GetProject(projectId);
                if (project == null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var factory = featureContextFactories.GetInterestedFeatureContextFactory(project, filePath);
                if (factory == null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var textBuffer = session.TextView.TextBuffer;
                var triggerPoint = session.GetTriggerPoint(textBuffer);

                var context = factory.CreateFeatureContext(project, filePath, triggerPoint.GetPosition(textBuffer.CurrentSnapshot));

                if (context == null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var syntax = context.GetSyntax<SyntaxNode>();

                var literalExpressionSyntax = syntax as LiteralExpressionSyntax;
                if (literalExpressionSyntax is null)
                {
                    if (syntax is ArgumentSyntax argumentSyntax
                        && argumentSyntax.Expression is LiteralExpressionSyntax inner)
                    {
                        literalExpressionSyntax = inner;
                    }
                }

                var span = GetStringSpan(literalExpressionSyntax);

                if (span == null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var imageName = GetImageName(literalExpressionSyntax, context.Project);

                if (string.IsNullOrEmpty(imageName)  // Exclude empty strings
                    || imageName.Contains(" ")) // Exclude any string with white space (invalid image name).
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }
                var imageAsset = imageAssetService.FindImageAsset(imageName, context.Solution);

                if (imageAsset == null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var trackingSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(Span.FromBounds(span.Value.Start, span.Value.End), SpanTrackingMode.EdgeInclusive);

                analyticsService.Track("C# Image Tooltip");

                ideImageManager.SetImageTooltipFile(imageAsset.PreviewImageFilePath);

                var imageId = new ImageId(ideImageManager.ImageTooltipId, ideImageManager.ImageTooltipId.GetHashCode());

                var image = new Microsoft.VisualStudio.Text.Adornments.ImageElement(imageId);

                return Task.FromResult(new QuickInfoItem(trackingSpan, imageId));
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return Task.FromResult<QuickInfoItem>(null);
        }

        string GetImageName(LiteralExpressionSyntax syntax, Project project)
        {
            var imageName = syntax.Token.ValueText;

            if (project.IsAndroidProject())
            {
                if (AndroidResourceNameHelper.ResourceReferenceRegex.IsMatch(imageName))
                {
                    var components = imageName.Split('/');

                    if (components.Length != 2)
                    {
                        return string.Empty;
                    }

                    var left = components[0];
                    var right = components[1];
                }
            }
            else if (project.IsAppleUnifiedProject())
            {
                return imageName;
            }

            return imageName;
        }

        TextSpan? GetStringSpan(SyntaxNode syntax)
        {
            if (syntax is LiteralExpressionSyntax literalExpressionSyntax)
            {
                if (literalExpressionSyntax.Token.IsKind(SyntaxKind.StringLiteralExpression)
                    || literalExpressionSyntax.Token.IsKind(SyntaxKind.StringLiteralToken))
                {
                    var span = syntax.Span;

                    return TextSpan.FromBounds(span.Start + 1, span.End - 1);
                }
            }

            return null;
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
}