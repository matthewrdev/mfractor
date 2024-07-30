using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ThicknessAttributeCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Thickness Attribute Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var thickness = CompletionHelper.ResolveShorthandCompletionTarget(context, textView.TextBuffer, triggerLocation);
            if (thickness == null)
            {
                return false;
            }

            var parentSymbol = context.XamlSemanticModel.GetSymbol(thickness) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(parentSymbol, context.Platform.Thickness.MetaType))
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>
            {
                CreateAttribute("Left", "The thickness of the left side of a rectangle."),
                CreateAttribute("Right", "The thickness of the right side of a rectangle."),
                CreateAttribute("Top", "The thickness of the top side of a rectangle."),
                CreateAttribute("Bottom", "The thickness of the bottom side of a rectangle.")
            };

            return items;
        }

        ICompletionSuggestion CreateAttribute(string name, string description)
        {
            var insertion = $"{name}=\"{CompletionHelper.CaretLocationMarker}\"";

            insertion = CompletionHelper.ExtractCaretLocation(insertion, out var caretOffset);

            var completion = new CompletionSuggestion(name, insertion);
            completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, description);
            completion.AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);

            return completion;
        }
    }
}
