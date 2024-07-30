using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class StarAutoShorthandCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Star/Auto Shorthand Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var node = context.GetSyntax<XmlNode>();

            // Is this contained within a <[Grid].[RowDefinitions][ColumnDefinitions]> setter?
            if (node == null || !node.HasParent)
            {
                return false;
            }

            if (!CompletionHelper.IsOnOpeningTag(node, textView.TextBuffer, triggerLocation.Position))
            {
                return false;
            }

            var nodeSetter = node.Parent;
            if (!XamlSyntaxHelper.ExplodePropertySetter(nodeSetter, out _, out var propertyName))
            {
                return false;
            }

            var targetType = context.XamlSemanticModel.GetSymbol(nodeSetter.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(targetType, context.Platform.Grid.MetaType))
            {
                return false;
            }

            var isGridElement = propertyName == context.Platform.RowDefinitionsProperty || propertyName == context.Platform.ColumnDefinitionsProperty;
            if (!isGridElement)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var platform = context.Platform;
            var node = context.GetSyntax<XmlNode>();

            var nodeSetter = node.Parent;
            XamlSyntaxHelper.ExplodePropertySetter(nodeSetter, out _, out var propertyName);

            var insertions = new List<ICompletionSuggestion>();

            if (propertyName == context.Platform.RowDefinitionsProperty)
            {
                insertions.Add(CreateInsertion(platform.RowProperty, platform.RowDefinition.Name, platform.RowHeightProperty, platform.GridNamedSize_Auto, platform.GridNamedSize_Auto));
                insertions.Add(CreateInsertion(platform.RowProperty, platform.RowDefinition.Name, platform.RowHeightProperty, platform.GridNamedSize_Star, platform.GridNamedSize_Star));
                if (platform.GridNamedSize_Star == "*")
                {
                    insertions.Add(CreateInsertion(platform.RowProperty, platform.RowDefinition.Name, platform.RowHeightProperty, "Star", platform.GridNamedSize_Star));
                }
            }
            else if (propertyName == context.Platform.ColumnDefinitionsProperty)
            {
                insertions.Add(CreateInsertion(platform.ColumnProperty, platform.ColumnDefinition.Name, platform.ColumnWidthProperty, platform.GridNamedSize_Auto, platform.GridNamedSize_Auto));
                insertions.Add(CreateInsertion(platform.ColumnProperty, platform.ColumnDefinition.Name, platform.ColumnWidthProperty, platform.GridNamedSize_Star, platform.GridNamedSize_Star));
                if (platform.GridNamedSize_Star == "*")
                {
                    insertions.Add(CreateInsertion(platform.ColumnProperty, platform.ColumnDefinition.Name, platform.ColumnWidthProperty, "Star", platform.GridNamedSize_Star));
                }
            }

            return insertions;
        }

        readonly string template = $"$definition$ $size$=\"$keyword$\"/>";
        ICompletionSuggestion CreateInsertion(string axisName, string definition, string size, string title, string keyword)
        {
            const string keywordTag = "$keyword$";
            const string definitionTag = "$definition$";
            const string sizeTag = "$size$";

            var insertion = template.Replace(keywordTag, keyword)
                                    .Replace(definitionTag, definition)
                                    .Replace(sizeTag, size);

            var item = new CompletionSuggestion(title, insertion);
            item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, $"Inserts a new {axisName} that uses {keyword}\n\nInserts:\n" + insertion);

            return item;
        }
    }
}
