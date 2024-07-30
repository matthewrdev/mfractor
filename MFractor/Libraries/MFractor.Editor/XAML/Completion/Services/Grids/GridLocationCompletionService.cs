using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Grids;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GridLocationCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Grid Location Completion";

        enum Mode
        {
            AttributeValue,
            Shorthand,
        }

        readonly Lazy<IGridAxisResolver> gridAxisResolver;
        public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

        [ImportingConstructor]
        public GridLocationCompletionService(Lazy<IGridAxisResolver> gridAxisResolver)
        {
            this.gridAxisResolver = gridAxisResolver;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var available = IsAttributeValueMode(textView, triggerLocation, context) || IsAttributeShorthandMode(textView, triggerLocation, context);

            return available;
        }

        bool IsAttributeValueMode(ITextView textView, SnapshotPoint triggerLocation, IXamlFeatureContext context)
        {
            if (!CompletionHelper.IsWithinAttributeValue(context, textView, triggerLocation))
            {
                return false;
            }

            var attribute = context.GetSyntax<XmlAttribute>();
            var field = context.XamlSemanticModel.GetSymbol(attribute) as IFieldSymbol;
            if (field == null)
            {
                return false;
            }

            if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return false;
            }

            var grid = attribute.Parent.Parent;
            var parentSymbol = context.XamlSemanticModel.GetSymbol(grid) as INamedTypeSymbol;

            return SymbolHelper.DerivesFrom(parentSymbol, context.Platform.Grid.MetaType);
        }

        bool IsAttributeShorthandMode(ITextView textView, SnapshotPoint triggerLocation, IXamlFeatureContext context)
        {
            var targetNode = CompletionHelper.ResolveShorthandCompletionTarget(context, textView.TextBuffer, triggerLocation);

            if (targetNode == null || !targetNode.HasParent)
            {
                return false;
            }

            var grid = targetNode.Parent;
            var parentSymbol = context.XamlSemanticModel.GetSymbol(grid) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(parentSymbol, context.Platform.Grid.MetaType))
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView,
                                                                     IXamlFeatureContext context,
                                                                     XamlExpressionSyntaxNode xamlExpression,
                                                                     SnapshotPoint triggerLocation,
                                                                     SnapshotSpan applicableToSpan,
                                                                     CancellationToken token)
        {
            IReadOnlyList<ICompletionSuggestion> result = Array.Empty<ICompletionSuggestion>();

            if (IsAttributeValueMode(textView, triggerLocation, context))
            {
                result = ProvideGridAttributeValueCompletion(context);
            }
            else if (IsAttributeShorthandMode(textView, triggerLocation, context))
            {
                var node = CompletionHelper.ResolveShorthandCompletionTarget(context, textView.TextBuffer, triggerLocation);

                result = ProvideGridShorthandCompletions(context, node, node.Parent);
            }

            return result;
        }

        IReadOnlyList<ICompletionSuggestion> ProvideGridAttributeValueCompletion(IXamlFeatureContext featureContext)
        {
            var attribute = featureContext.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var field = featureContext.XamlSemanticModel.GetSymbol(attribute) as IFieldSymbol;
            if (field == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var grid = attribute.Parent.Parent;

            return ProvideGridCompletions(featureContext, field, attribute.Parent, grid, Mode.AttributeValue);
        }

        IReadOnlyList<ICompletionSuggestion> ProvideGridCompletions(IXamlFeatureContext context,
                                                           IFieldSymbol field,
                                                           XmlNode element,
                                                           Xml.XmlNode gridNode,
                                                           Mode mode)
        {
            var platform = context.Platform;

            if (field.Name == $"{platform.RowProperty}Property" && SymbolHelper.DerivesFrom(field.ContainingType, platform.Grid.MetaType))
            {
                return ProvideRowCompletions(context, gridNode, mode);
            }
            else if (field.Name == $"{platform.ColumnProperty}Property" && SymbolHelper.DerivesFrom(field.ContainingType, platform.Grid.MetaType))
            {
                return ProvideColumnCompletions(context, gridNode, mode);
            }
            else if (field.Name == $"{platform.ColumnProperty}SpanProperty" && SymbolHelper.DerivesFrom(field.ContainingType, platform.Grid.MetaType))
            {
                return ProvideColumnSpanCompletions(context, gridNode, element, mode);
            }
            else if (field.Name == $"{platform.RowProperty}SpamProperty" && SymbolHelper.DerivesFrom(field.ContainingType, platform.Grid.MetaType))
            {
                return ProvideRowSpanCompletions(context, gridNode, element, mode);
            }

            return Array.Empty<ICompletionSuggestion>();
        }

        IReadOnlyList<ICompletionSuggestion> ProvideColumnCompletions(IXamlFeatureContext context,
                                                             Xml.XmlNode grid,
                                                             Mode mode)
        {
            var items = new List<ICompletionSuggestion>();

            var columns = GridAxisResolver.ResolveColumnDefinitions(grid, context.Platform).ToList();
            if (!columns.Any())
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var icon = new ImageElement(IconIds.GridColumn);
            foreach (var column in columns)
            {
                var index = column.Index.ToString();
                var insertion = column.Index.ToString();

                if (mode == Mode.Shorthand)
                {
                    insertion = $"Grid.Column=\"{index}\"";
                }

                var displayText = "Column \"" + index + "\"";
                if (mode == Mode.Shorthand)
                {
                    displayText = insertion;
                }
                var description = "Use the column at index " + index;

                description += " with the width of \"" + column.Value + "\"";
                displayText += " - Width \"" + column.Value+ "\"";

                if (column.HasName)
                {
                    displayText += $" ({column.Name})";
                }

                var item = new CompletionSuggestion(displayText, insertion, icon);
                item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, description);

                items.Add(item);
            }

            return items;
        }

        IReadOnlyList<ICompletionSuggestion> ProvideColumnSpanCompletions(IXamlFeatureContext context,
                                                                 Xml.XmlNode grid,
                                                                 XmlNode element,
                                                                 Mode mode)
        {
            var column = element.GetAttribute(a => a.Name.LocalName == $"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}");
            if (column == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            int.TryParse(column?.Value?.Value, out var columnIndex);

            var columns = GridAxisResolver.ResolveColumnDefinitions(grid, context.Platform).ToList();
            if (!columns.Any())
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var items = new List<ICompletionSuggestion>();

            var icon = new ImageElement(IconIds.GridColumn);
            if (columnIndex < columns.Count)
            {
                for (var i = columnIndex; i < columns.Count; i++)
                {
                    var span = 1 + i - columnIndex;

                    var insertion = span.ToString();
                    if (mode == Mode.Shorthand)
                    {
                        insertion = $"Grid.ColumnSpan=\"{span}\"";
                    }

                    var displayText = "ColumnSpan \"" + span + "\"";
                    if (mode == Mode.Shorthand)
                    {
                        displayText = insertion;
                    }
                    var tooltip = $"This element is currently at column {columnIndex}.\n\nUsing a span of {span} will cover these columns:{Environment.NewLine}";

                    var values = new List<string>();
                    for (var j = columnIndex; j < columnIndex + span; ++j)
                    {
                        var c = columns[j];

                        var content = $" * Column \"{j}\" - Width \"{c.Value}\"";

                        if (c.HasName)
                        {
                            content += $" ({c.Name})";
                        }

                        values.Add(content);
                    }

                    tooltip += string.Join("\n", values);

                    var item = new CompletionSuggestion(displayText, insertion, icon);
                    item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip);

                    items.Add(item);
                }
            }

            return items;
        }

        IReadOnlyList<ICompletionSuggestion> ProvideRowSpanCompletions(IXamlFeatureContext context,
                                                              XmlNode grid,
                                                              XmlNode element,
                                                              Mode mode)
        {

            var row = element.GetAttribute(a => a.Name.LocalName == $"{context.Platform.Grid.Name}.{context.Platform.RowProperty}");
            if (row == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            int.TryParse(row?.Value?.Value, out var rowIndex);


            var rows = GridAxisResolver.ResolveRowDefinitions(grid, context.Platform).ToList();
            if (!rows.Any())
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var items = new List<ICompletionSuggestion>();

            var icon = new ImageElement(IconIds.GridRow);
            if (rowIndex < rows.Count)
            {
                for (var i = rowIndex; i < rows.Count; i++)
                {
                    var span = 1 + i - rowIndex;

                    var insertion = span.ToString();
                    if (mode == Mode.Shorthand)
                    {
                        insertion = $"Grid.RowSpan=\"{span}\"";
                    }

                    var displayText = "RowSpan \"" + span + "\"";
                    if (mode == Mode.Shorthand)
                    {
                        displayText = insertion;
                    }

                    var tooltip = $"This element is currently at row {rowIndex}.\n\nUsing a span of {span} will cover these rows:\n";

                    var values = new List<string>();
                    for (var j = rowIndex; j < rowIndex + span; ++j)
                    {
                        var r = rows[j];
                        var content = " * Row \"" + j + "\"" + " - Height \"" + r.Value+ "\"";

                        if (r.HasName)
                        {
                            content += $" ({r.Name})";
                        }

                        values.Add(content);
                    }

                    tooltip += string.Join("\n", values);

                    var item = new CompletionSuggestion(displayText, insertion, icon);
                    item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip);

                    items.Add(item);
                }
            }

            return items;
        }

        IReadOnlyList<ICompletionSuggestion> ProvideRowCompletions(IXamlFeatureContext context,
                                                          XmlNode grid,
                                                          Mode mode)
        {
            var rowDefinitions = GridAxisResolver.ResolveRowDefinitions(grid, context.Platform);

            if (!rowDefinitions.Any())
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var items = new List<ICompletionSuggestion>();

            var icon = new ImageElement(IconIds.GridRow);
            foreach (var row in rowDefinitions)
            {
                var index = row.Index.ToString();
                var insertion = row.Index.ToString();

                if (mode == Mode.Shorthand)
                {
                    insertion = $"Grid.Row=\"{index}\"";
                }

                var displayText = "Row \"" + index + "\"";
                if (mode == Mode.Shorthand)
                {
                    displayText = insertion;
                }

                var description = "Use the row at index " + index;

                description += " with the height of \"" + row.Value + "\"";
                displayText += " - Height \"" + row.Value + "\"";

                if (row.HasName)
                {
                    displayText += $" ({row.Name})";
                }

                var item = new CompletionSuggestion(displayText, insertion, icon);
                item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, description);

                items.Add(item);
            }

            return items;
        }

        IReadOnlyList<ICompletionSuggestion> ProvideGridShorthandCompletions(IXamlFeatureContext context,
                                                                           XmlNode element,
                                                                           XmlNode grid)
        {
            if (element == null || grid == null)
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var items = new List<ICompletionSuggestion>();

            if (!element.HasAttribute($"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}"))
            {
                items.AddRange(ProvideColumnCompletions(context, grid, Mode.Shorthand));
            }

            if (!element.HasAttribute($"{context.Platform.Grid.Name}.{context.Platform.ColumnProperty}Span"))
            {
                items.AddRange(ProvideColumnSpanCompletions(context, grid, element, Mode.Shorthand));
            }

            if (!element.HasAttribute($"{context.Platform.Grid.Name}.{context.Platform.RowProperty}"))
            {
                items.AddRange(ProvideRowCompletions(context, grid, Mode.Shorthand));
            }

            if (!element.HasAttribute($"{context.Platform.Grid.Name}.{context.Platform.RowProperty}Span"))
            {
                items.AddRange(ProvideRowSpanCompletions(context, grid, element, Mode.Shorthand));
            }

            return items;
        }
    }
}
