using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Tooltips;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDynamicResourceCompletionService))]
    class DynamicResourceCompletionService : IAsyncXamlCompletionService, IDynamicResourceCompletionService
    {
        readonly Lazy<IDynamicResourceResolver> dynamicResourceResolver;
        public IDynamicResourceResolver DynamicResourceResolver => dynamicResourceResolver.Value;

        readonly Lazy<IDynamicResourceTooltipRenderer> dynamicResourceTooltipRenderer;
        public IDynamicResourceTooltipRenderer DynamicResourceTooltipRenderer => dynamicResourceTooltipRenderer.Value;

        public string AnalyticsEvent => "DynamicResource Shorthand Completion";

        [ImportingConstructor]
        public DynamicResourceCompletionService(Lazy<IDynamicResourceResolver> dynamicResourceResolver,
                                                Lazy<IDynamicResourceTooltipRenderer> dynamicResourceTooltipRenderer)
        {
            this.dynamicResourceResolver = dynamicResourceResolver;
            this.dynamicResourceTooltipRenderer = dynamicResourceTooltipRenderer;
        }

        public Task<bool> CanProvideCompletionsAsync(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return Task.FromResult(false);
            }

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
            if (property.Name == "AutomationId"
                && SymbolHelper.DerivesFrom(property.ContainingType, context.Platform.Element.MetaType))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<ICompletionSuggestion>> ProvideCompletionsAsync(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return Task.FromResult(ProvideDynamicResourceCompletions(context, true));
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideDynamicResourceCompletions(IXamlFeatureContext featureContext, bool isShorthandMode)
        {
            var items = new List<ICompletionSuggestion>();

            var result = DynamicResourceResolver.GetAvailableDynamicResources(featureContext);

            var icon = new ImageElement(IconIds.DynamicResourceBox);

            foreach (var symbol in result.DistinctBy(r => r.Definition.Name))
            {
                var insertion = isShorthandMode ? "{DynamicResource " + symbol.Definition.Name + "}" : symbol.Definition.Name;

                var tooltip = DynamicResourceTooltipRenderer.CreateTooltip(symbol.Definition.Name, symbol.Project);

                var completion = new CompletionSuggestion(symbol.Definition.Name, insertion, icon);

                completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip);

                items.Add(completion);
            }

            return items;
        }
    }

}
