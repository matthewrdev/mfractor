using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Code;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using MFractor.Code.Formatting;

namespace MFractor.Editor.XAML.Completion.Services.Grids
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GridBoilerplateCompletionService : IXamlCompletionService
    {
        const string gridArgumentName = "$grid$";
        const string columnDefinitionsArgumentName = "$column_definitions$";
        const string rowDefinitionsArgumetnName = "$row_definitions$";

        const string gridTemplate = "$grid$>\n"
                                  + "   <$grid$.$column_definitions$>\n"
                                  + "   </$grid$.$column_definitions$>\n"
                                  + "   <$grid$.$row_definitions$>\n"
                                  + "   </$grid$.$row_definitions$>\n"
                                  + "   " + CompletionHelper.CaretLocationMarker + "\n"
                                  + "</$grid$>";

        const string gridAltTemplate = "$grid$ $column_definitions$=\"Auto\" $row_definitions$=\"Auto\">\n"
                                  + "   " + CompletionHelper.CaretLocationMarker + "\n"
                                  + "</$grid$>";

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        [ImportingConstructor]
        public GridBoilerplateCompletionService(Lazy<IAnalyticsService> analyticsService,
                                                Lazy<IWorkEngine> workEngine,
                                                Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                                Lazy<ICodeFormattingPolicyService> formattingPolicyService)
        {
            this.analyticsService = analyticsService;
            this.workEngine = workEngine;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
        }

        public string AnalyticsEvent => "Grid Boilerplate Shorthand Completion";

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

            var parent = node.Parent;
            var type = context.XamlSemanticModel.GetSymbol(parent) as INamedTypeSymbol;

            var isCandidate = SymbolHelper.DerivesFrom(type, context.Platform.Layout.MetaType)
                              || SymbolHelper.DerivesFrom(type, context.Platform.Page.MetaType)
                              || SymbolHelper.DerivesFrom(type, context.Platform.DataTemplate.MetaType)
                              || SymbolHelper.DerivesFrom(type, context.Platform.ResourceDictionary.MetaType)
                              || SymbolHelper.DerivesFrom(type, context.Platform.ViewCell.MetaType);
            if (!isCandidate)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();
            BuildBoilerplateInsertion(textView, context, triggerLocation, items);
            BuildAltBoilerplateInsertion(textView, context, triggerLocation, items);

            return items;
        }

        private static void BuildBoilerplateInsertion(ITextView textView, IXamlFeatureContext context, SnapshotPoint triggerLocation, List<ICompletionSuggestion> items)
        {
            var actionName = "Boilerplate Grid";

            var textBuffer = textView.TextBuffer;

            var nodeName = textBuffer.GetCurrentNodeNameSpanAtOffset(triggerLocation.Position);
            var location = textBuffer.GetLocation(nodeName.Start);

            var code = gridTemplate;

            var insertion = code.Replace(gridArgumentName, context.Platform.Grid.Name)
                                .Replace(columnDefinitionsArgumentName, context.Platform.ColumnDefinitionsProperty)
                                .Replace(rowDefinitionsArgumetnName, context.Platform.RowDefinitionsProperty)
                                .Replace("\n", "\n" + (new string(' ', location.Column - 1)));

            insertion = CompletionHelper.ExtractCaretLocation(insertion, out var caretOffset);

            var action = new CompletionSuggestion(actionName, insertion);
            action.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Add a new grid declaration containing empty row and column definitions.\n\nInserts:\n" + gridTemplate);
            action.AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);
            items.Add(action);
        }
        private static void BuildAltBoilerplateInsertion(ITextView textView, IXamlFeatureContext context, SnapshotPoint triggerLocation, List<ICompletionSuggestion> items)
        {
            var actionName = "Boilerplate Grid (Attributes)";

            var textBuffer = textView.TextBuffer;

            var nodeName = textBuffer.GetCurrentNodeNameSpanAtOffset(triggerLocation.Position);
            var location = textBuffer.GetLocation(nodeName.Start);

            var code = gridAltTemplate;

            var insertion = code.Replace(gridArgumentName, context.Platform.Grid.Name)
                                .Replace(columnDefinitionsArgumentName, context.Platform.ColumnDefinitionsProperty)
                                .Replace(rowDefinitionsArgumetnName, context.Platform.RowDefinitionsProperty)
                                .Replace("\n", "\n" + (new string(' ', location.Column - 1)));

            insertion = CompletionHelper.ExtractCaretLocation(insertion, out var caretOffset);

            var action = new CompletionSuggestion(actionName, insertion);
            action.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Add a new grid declaration containing empty row and column definitions attributes.\n\nInserts:\n" + gridTemplate);
            action.AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);
            items.Add(action);
        }
    }
}
