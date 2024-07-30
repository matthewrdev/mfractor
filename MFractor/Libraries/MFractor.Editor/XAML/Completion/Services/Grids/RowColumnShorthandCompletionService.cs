using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
    class RowColumnShorthandCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Row/Column Shorthand Completion";

        readonly string rowTemplate = $"RowDefinition Height=\"{CompletionHelper.CaretLocationMarker}\"/>";
        readonly string columnTemplate = $"ColumnDefinition Width=\"{CompletionHelper.CaretLocationMarker}\"/>";

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
                return default;
            }

            var nodeSetter = node.Parent;
            if (!XamlSyntaxHelper.ExplodePropertySetter(nodeSetter, out var className, out var propertyName))
            {
                return false;
            }

            var targetType = context.XamlSemanticModel.GetSymbol(nodeSetter.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(targetType, context.Platform.Grid.MetaType))
            {
                return false;
            }

            var isGridElement = propertyName == context.Platform.RowDefinitionsProperty || propertyName == context.Platform.ColumnDefinitionsProperty;

            return isGridElement;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var node = context.GetSyntax<XmlNode>();
            var platform = context.Platform;

            var nodeSetter = node.Parent;
            XamlSyntaxHelper.ExplodePropertySetter(nodeSetter, out _, out var propertyName);

            IReadOnlyList<ICompletionSuggestion> insertions = Array.Empty<ICompletionSuggestion>();

            if (propertyName == context.Platform.RowDefinitionsProperty)
            {
                insertions = CreateInsertions(platform.RowDefinition.Name, platform.RowHeightProperty, platform.RowProperty).AsList();
            }
            else if (propertyName == context.Platform.ColumnDefinitionsProperty)
            {
                insertions = CreateInsertions(platform.ColumnDefinition.Name, platform.ColumnWidthProperty, platform.ColumnProperty).AsList();
            }

            return insertions;
        }

        readonly string template = $"$definition$ $dimension$=\"{CompletionHelper.CaretLocationMarker}\"/>";
        ICompletionSuggestion CreateInsertions(string definition, string dimension, string title)
        {
            const string definitionTag = "$definition$";
            const string dimensionTag = "$dimension$";

            var insertion = template.Replace(definitionTag, definition)
                                    .Replace(dimensionTag, dimension);

            insertion = CompletionHelper.ExtractCaretLocation(insertion, out var caretOffset);

            var item = new CompletionSuggestion(title, insertion);
            item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, $"Inserts a new {title}, placing the caret within the {dimension} attribute.\n\nInserts:\n" + insertion);
            item.AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);

            return item;
        }
    }
}
