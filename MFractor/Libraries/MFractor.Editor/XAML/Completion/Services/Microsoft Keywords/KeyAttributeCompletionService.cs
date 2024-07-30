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
    class KeyAttributeCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "x:Key Shorthand Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var element = CompletionHelper.ResolveShorthandCompletionTarget(context, textView.TextBuffer, triggerLocation);
            if (element == null || !element.HasParent)
            {
                return false;
            }

            var parentSymbol = context.XamlSemanticModel.GetSymbol(element.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(parentSymbol,context.Platform.ResourceDictionary.MetaType))
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>
            {
                CreateAttribute("Key", "Declares the key for this resource dictionary entry.")
            };

            return items;
        }

        ICompletionSuggestion CreateAttribute(string name, string description)
        {
            var insertion = $"x:{name}=\"{CompletionHelper.CaretLocationMarker}\"";

            insertion = CompletionHelper.ExtractCaretLocation(insertion, out var caretOffset);

            var completion = new CompletionSuggestion(name, insertion);
            completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, description);
            completion.AddProperty(XamlCompletionItemPropertyKeys.CaretOffset, caretOffset);

            return completion;
        }
    }
}
