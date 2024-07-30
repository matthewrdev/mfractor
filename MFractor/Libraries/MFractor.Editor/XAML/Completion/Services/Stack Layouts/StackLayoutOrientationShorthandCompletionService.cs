using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class StackLayoutOrientationShorthandCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "StackLayout Orientation Shorthand Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var stackLayout = CompletionHelper.ResolveShorthandCompletionTarget(context, textView.TextBuffer, triggerLocation);
            if (stackLayout == null)
            {
                return false;
            }

            var parentSymbol = context.XamlSemanticModel.GetSymbol(stackLayout) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(parentSymbol, context.Platform.StackLayout.MetaType))
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();

            var vertical = new CompletionSuggestion("Vertical (Orientation)", "Orientation=\"Vertical\"");
            vertical.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Set the StackLayout to use a vertical orientation.\n\nInserts:\nOrientation=\"Vertical\"");
            items.Add(vertical);

            var horizontal = new CompletionSuggestion("Horizontal (Orientation)", "Orientation=\"Horizontal\"");
            horizontal.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Set the StackLayout to use a horizontal orientation.\n\nInserts:\nOrientation=\"Horizontal\"");
            items.Add(horizontal);

            return items;
        }
    }
}
