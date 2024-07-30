using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Images;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageAssetCompletionService))]
    class ImageAssetCompletionService : IXamlCompletionService, IImageAssetCompletionService
    {
        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IImageAssetService> imageAssetService;
        public IImageAssetService ImageAssetService => imageAssetService.Value;

        public string AnalyticsEvent => "Image Asset Completion";

        [ImportingConstructor]
        public ImageAssetCompletionService(Lazy<IAnalyticsService> analyticsService,
                                           Lazy<IWorkEngine> workEngine,
                                           Lazy<IImageAssetService> imageAssetService)
        {
            this.analyticsService = analyticsService;
            this.workEngine = workEngine;
            this.imageAssetService = imageAssetService;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var isImageAsset = IsImageAssetCompletionContext(context);

            return isImageAsset;
        }

        bool IsImageAssetCompletionContext(IXamlFeatureContext featureContext)
        {
            var returnType = GetAttributeMemberReturnType(featureContext);

            return SymbolHelper.DerivesFrom(returnType, featureContext.Platform.ImageSource.MetaType);
        }

        ITypeSymbol GetAttributeMemberReturnType(IXamlFeatureContext context)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return null;
            }

            if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(attribute);

            return SymbolHelper.ResolveMemberReturnType(symbol);
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();

            items.AddRange(ProvideImageActionCompletions(textView, context));

            items.AddRange(ProvideImageAssetCompletions(context));

            return items;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideImageActionCompletions(ITextView textView, IXamlFeatureContext context)
        {
            var items = new List<ICompletionSuggestion>();

            var actionName = "Import an image asset";

            var action = new CompletionSuggestion(actionName, new ImageElement(IconIds.Wand));
            action.AddProperty(XamlCompletionItemPropertyKeys.CompletionAction, new CompletionAction((tv, t, ci) =>
            {
                IReadOnlyList<IWorkUnit> onImageNameConfirmed(string imageName)
                {
                    var span = TextEditorHelper.GetAttributeSpanAtOffset(textView.TextBuffer, tv.GetCaretOffset());

                    imageName = ImageNameHelper.GetImageAssetName(context.Project, imageName);

                    return new ReplaceTextWorkUnit()
                    {
                        Span = span,
                        Text = imageName,
                        FilePath = context.Document.FilePath,
                    }.AsList();
                }

                var chooseImageWorkUnit = new ImportImageAssetWorkUnit
                {
                    OnImageNameConfirmedCallback = onImageNameConfirmed
                };

                var projects = context.Project.Solution.GetMobileProjects().Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == context.Project.Id)).ToList();

                chooseImageWorkUnit.Projects = projects;

                AnalyticsService.Track("Import image asset completion");

                return chooseImageWorkUnit.AsList();

            }));

            action.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Import a new image asset into the Android and iOS projects that reference " + context.Project?.Name);
            action.AddProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent, "Import Image Completion");
            items.Add(action);

            return items;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideImageAssetCompletions(IXamlFeatureContext context)
        {
            var items = new List<ICompletionSuggestion>();

            var projects = context.Project.GetDependentMobileProjects().ToList();

            var assets = ImageAssetService.GatherImageAssets(projects);

            var icon = new ImageElement(IconIds.Image);
            foreach (var asset in assets)
            {
                var imageAsset = asset.Value;

                if (imageAsset.ImageAssetKinds.Contains(ImageAssetKind.AppIcon))
                {
                    continue;
                }

                var tooltip = $"The image asset {imageAsset.Name} with the following image resources:\n\n";

                foreach (var project in imageAsset.Projects)
                {
                    var densities = imageAsset.GetAssetsFor(project);

                    tooltip += project.Name + Environment.NewLine;

                    tooltip += string.Join(Environment.NewLine, densities.Select(d => " * " + d.VirtualPath));

                    if (imageAsset.Projects.Last() != project)
                    {
                        tooltip += Environment.NewLine;
                    }
                }

                var item = new CompletionSuggestion(asset.Key, icon);
                item.AddProperty(XamlCompletionItemPropertyKeys.TooltipModel, new ImageTooltipModel(imageAsset))
                    .AddProperty(XamlCompletionItemPropertyKeys.TooltipText, asset.Value);

                items.Add(item);
            }

            return items;
        }
    }
}
