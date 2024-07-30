using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Data;
using MFractor.Editor.Tooltips;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Tooltips;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IStaticResourceCompletionService))]
    class StaticResourceCompletionService : IAsyncXamlCompletionService, IStaticResourceCompletionService
    {
        readonly Lazy<IStaticResourceTooltipRenderer> staticResourceTooltipRenderer;
        public IStaticResourceTooltipRenderer StaticResourceTooltipRenderer => staticResourceTooltipRenderer.Value;

        readonly Lazy<IStaticResourceResolver> staticResourceResolver;
        public IStaticResourceResolver StaticResourceResolver => staticResourceResolver.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        public string AnalyticsEvent => "StaticResource Shorthand Completion";

        [ImportingConstructor]
        public StaticResourceCompletionService(Lazy<IStaticResourceResolver> staticResourceResolver,
                                               Lazy<IStaticResourceTooltipRenderer> staticResourceTooltipRenderer,
                                               Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.staticResourceResolver = staticResourceResolver;
            this.staticResourceTooltipRenderer = staticResourceTooltipRenderer;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public Task<bool> CanProvideCompletionsAsync(ITextView textView,
                                                     IXamlFeatureContext context,
                                                     XamlExpressionSyntaxNode xamlExpression,
                                                     SnapshotPoint triggerLocation,
                                                     SnapshotSpan applicableToSpan,
                                                     CancellationToken token)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return Task.FromResult(false);
            }

            if (!CompletionHelper.IsWithinAttributeValue(attribute, textView.TextBuffer, triggerLocation.Position))
            {
                return Task.FromResult(false);
            }

            var node = attribute.Parent;

            var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;
            if (property == null)
            {
                return Task.FromResult(false);
            }

            if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return Task.FromResult(false);
            }

            // Ignore automation ids as we can't data bind to them.
            if (property.Name == "AutomationId" && SymbolHelper.DerivesFrom(property.ContainingType, context.Platform.Element.MetaType))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<ICompletionSuggestion>> ProvideCompletionsAsync(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;

            return ProvideStaticResourceCompletions(context, property, true);
        }

        public async Task<IReadOnlyList<ICompletionSuggestion>> ProvideStaticResourceCompletions(IXamlFeatureContext context, IPropertySymbol property, bool isShorthand)
        {
            if (property is null
                || context is null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var items = new List<ICompletionSuggestion>();

            var typeSymbol = property.Type;

            var isThickness = SymbolHelper.DerivesFrom(typeSymbol, context.Platform.Thickness.MetaType);

            var resources = StaticResourceResolver.GetAvailableResources(context.Project, context.Platform, context.Document.FilePath);
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);

            if (database is null || !database.IsValid)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var colorDatabase = database.GetRepository<ColorDefinitionRepository>();

            var icon = new ImageElement(IconIds.FeatherBox);

            var names = new HashSet<string>();

            foreach (var definition in resources)
            {
                if (definition.IsImplicitResource)
                {
                    continue;
                }

                if (names.Contains(definition.Name))
                {
                    continue;
                }

                names.Add(definition.Name);

                var namedType = context.Compilation.GetTypeByMetadataName(definition.ReturnType);

                var isTypeCompatible = SymbolHelper.DerivesFrom(namedType, typeSymbol) || typeSymbol == null;

                if (namedType != null && isThickness)
                {
                    if (!SymbolHelper.DerivesFrom(namedType, context.Platform.Thickness.MetaType))
                    {
                        isTypeCompatible = (namedType.SpecialType == SpecialType.System_Double || namedType.SpecialType == SpecialType.System_Decimal);
                    }
                }

                if (isTypeCompatible)
                {
                    var insertion = isShorthand ? "{" + context.Platform.StaticResourceExtension.MarkupExpressionName + " " + definition.Name + "}" : definition.Name;

                    var completion = new CompletionSuggestion(definition.Name, insertion, icon);

                    var colorDefinition = colorDatabase.GetColorForStaticResourceDefinition(definition);

                    var project = resources.GetProjectFor(definition);
                    var tooltip = StaticResourceTooltipRenderer.CreateTooltip(definition, project, colorDefinition == null);
                    if (colorDefinition != null)
                    {
                        completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipModel, new ColorTooltipModel(colorDefinition.Color, tooltip));
                    }
                    else
                    {
                        completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip);
                    }

                    items.Add(completion);
                }
            }

            return items;
        }
    }
}
