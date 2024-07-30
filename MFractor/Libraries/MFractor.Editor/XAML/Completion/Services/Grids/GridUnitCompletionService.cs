using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GridUnitCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Grid Unit Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context,  XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var isGridSize = IsGridSizeContext(context, textView, triggerLocation);

            return isGridSize;
        }

        bool IsGridSizeContext(IXamlFeatureContext context, ITextView textView, SnapshotPoint triggerLocation)
        {
            var attribute = context.GetSyntax<XmlAttribute>();

            if (!CompletionHelper.IsWithinAttributeValue(context, textView, triggerLocation))
            {
                return false;
            }

            if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return false;
            }

            var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;
            if (property == null)
            {
                return false;
            }

            if (property.Name != context.Platform.RowHeightProperty && property.Name != context.Platform.ColumnWidthProperty)
            {
                return false;
            }

            return SymbolHelper.DerivesFrom(property.ContainingType, context.Platform.RowDefinition.MetaType)
                  || SymbolHelper.DerivesFrom(property.ContainingType, context.Platform.ColumnDefinition.MetaType);
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();

            var star = new CompletionSuggestion("*", "*");

            star.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Interpret as a proportional weight, to be laid out after rows and columns with GridUnitType.Absolute or GridUnitType.Auto are accounted for.");
            items.Add(star);

            var starAlias = new CompletionSuggestion("Star", "*");

            starAlias.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Star - An alias for '*'\n\nInterpret as a proportional weight, to be laid out after rows and columns with GridUnitType.Absolute or GridUnitType.Auto are accounted for.");
            items.Add(starAlias);

            var auto = new CompletionSuggestion("Auto", "Auto");

            auto.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Choose a size that fits the children of the row or column.");
            items.Add(auto);


            return items;
        }
    }
}
