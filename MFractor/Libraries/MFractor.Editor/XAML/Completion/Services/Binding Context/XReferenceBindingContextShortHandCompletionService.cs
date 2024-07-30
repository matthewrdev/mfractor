using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services.BindingContext
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class XReferenceBindingContextShortHandCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "x:Reference BindingContext Shorthand Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null || attribute.Name.FullName != context.Platform.BindingContextProperty)
            {
                return false;
            }

            if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return false;
            }

            return context.XamlSemanticModel.CodeBehindFields.Any();
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var fields = context.XamlSemanticModel.CodeBehindFields;

            var completions = new List<ICompletionSuggestion>();

            foreach (var field in fields)
            {
                var insertion = "{x:Reference " + field.Name + "}";
                var completion = new CompletionSuggestion(field.Name, insertion);

                var tooltip = "Use the code-behind field " + field.Name + " as the binding context.\n\nInserts:\n" + insertion;
                completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, tooltip);

                completions.Add(completion);
            }

            return completions;
        }
    }
}
