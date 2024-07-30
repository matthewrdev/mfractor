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
    class CodeBehindFieldNameCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "x:Name Shorthand Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var node = CompletionHelper.ResolveShorthandCompletionTarget(context, textView.TextBuffer, triggerLocation);
            if (node == null)
            {
                return false;
            }

            var parentSymbol = context.XamlSemanticModel.GetSymbol(node) as INamedTypeSymbol;
            if (parentSymbol == null)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return CreateAttribute("Name", "Specifies a runtime object name for the XAML element. Setting x:Name is similar to declaring a variable in code.").AsList();
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
