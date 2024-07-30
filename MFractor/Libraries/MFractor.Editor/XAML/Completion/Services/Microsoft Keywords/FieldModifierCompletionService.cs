using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class FieldModifierCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "DataBinding Shorthand Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return false;
            }

            if (attribute.Name.Namespace != "x" || attribute.Name.LocalName != "FieldModifier")
            {
                return false;
            }

            var preceeding = textView.TextBuffer.GetFirstNonWhitespacePreceedingCharacter(triggerLocation.Position);
            if (preceeding != "\"")
            {
                return false;
            }

            if (attribute.HasValue)
            {
                if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>
            {
                CreateAttribute("private", "Specifies that the generated field for the XAML element is accessible only within the body of the class in which it is declared."),
                CreateAttribute("public", "Specifies that the generated field for the XAML element has no access restrictions."),
                CreateAttribute("protected", "Specifies that the generated field for the XAML element is accessible within its class and by derived class instances."),
                CreateAttribute("internal", "Specifies that the generated field for the XAML element is accessible only within types in the same assembly."),
                CreateAttribute("notpublic", "Specifies that the generated field for the XAML element is accessible only within types in the same assembly.")
            };

            return items;
        }

        ICompletionSuggestion CreateAttribute(string name, string description)
        {
            var completion = new CompletionSuggestion(name, name);
            completion.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, description);

            return completion;
        }
    }
}
