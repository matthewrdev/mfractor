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

namespace MFractor.Editor.XAML.Completion.Services.StackLayouts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class StackLayoutBoilerplateCompetionService : IXamlCompletionService
    {
        const string orientationArgument = "$orientation$";

        readonly string template = $"StackLayout Orientation=\"{orientationArgument}\">\n"
                                  + $"  {CompletionHelper.CaretLocationMarker}\n"
                                  + "</StackLayout>";

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        [ImportingConstructor]
        public StackLayoutBoilerplateCompetionService(Lazy<IAnalyticsService> analyticsService,
                                                     Lazy<IWorkEngine> workEngine,
                                                     Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                                     Lazy<ICodeFormattingPolicyService> formattingPolicyService)
        {
            this.analyticsService = analyticsService;
            this.workEngine = workEngine;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
        }

        public string AnalyticsEvent => "StackLayout Boilerplate Shorthand Completion";

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

            var platform = context.Platform;
            var isCandidate = SymbolHelper.DerivesFrom(type, platform.Layout.MetaType)
                              || SymbolHelper.DerivesFrom(type, platform.Page.MetaType)
                              || SymbolHelper.DerivesFrom(type, platform.DataTemplate.MetaType)
                              || SymbolHelper.DerivesFrom(type, platform.ResourceDictionary.MetaType)
                              || SymbolHelper.DerivesFrom(type, platform.ViewCell.MetaType);
            if (!isCandidate)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>
            {
                CreateBoilerplateCompletion("Horizontal", textView.TextBuffer, triggerLocation),
                CreateBoilerplateCompletion("Vertical", textView.TextBuffer, triggerLocation)
            };

            return items;
        }

        ICompletionSuggestion CreateBoilerplateCompletion(string orientation, ITextBuffer textBuffer, SnapshotPoint triggerLocation)
        {
            var actionName = $"StackLayout ({orientation})";

            var nodeName = textBuffer.GetCurrentNodeNameSpanAtOffset(triggerLocation.Position);
            var location = textBuffer.GetLocation(nodeName.Start);

            var code = template.Replace(orientationArgument, orientation);

            var insertion = code.Replace("\n", "\n" + (new string(' ', location.Column - 1)));

            insertion = CompletionHelper.ExtractCaretLocation(insertion, out var caretOffset);

            var item = new CompletionSuggestion(actionName, insertion);
            item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, $"Create a {orientation.FirstCharToLower()} StackLayout that uses the {orientation} orientation.\n\nInserts:\n" + code.Replace(CompletionHelper.CaretLocationMarker, string.Empty));
            item.AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);

            return item;
        }
    }
}
