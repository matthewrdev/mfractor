using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Analysis.Expressions
{
    class CodeBehindFieldReferenceFixBundle
    {
        public ReferenceExpression ReferenceExpression;
        public string BestMatch;
    }

    class CodeBehindFieldReferenceDoesNotResolveAnalysis : ExpressionTreeAnalysisRoutine<ReferenceExpression>
    {
        public override string Documentation => "Inspects usages of the `x:Reference` expression and validates the referenced element has been declared within the document. `x:Reference` expressions are used to resolve another Xaml node has a code behind field defined using the `x:Name` attribute.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.xreference_element_exists";

        public override string Name => "Referenced Code Behind Field Exists";

        public override string DiagnosticId => "MF1015";

        public override bool ShouldInspectExpressionChildren(Expression expression, int expressionDepth)
        {
            if (expression is BindingModeExpression
               || expression is StringFormatExpression
                || expression is PathExpression)
            {
                return false;
            }

            return true;
        }

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(ReferenceExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var value = "";
            var valueSpan = default(TextSpan);
            if (expression.NameExpression != null)
            {
                value = expression.NameExpression.NameValue.Value;
                valueSpan = expression.NameExpression.AssignmentValue.Span;
            }
            else if (expression.Value != null)
            {
                value = expression.Value.Value;
                valueSpan = expression.Value.Span;
            }

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var codeBehindFields = context.XamlSemanticModel.CodeBehindFields;

            var node = codeBehindFields.GetCodeBehindField(value);

            if (node != null)
            {
                return null;
            }

            var nearestMatch = SuggestionHelper.FindBestSuggestion(value, codeBehindFields.CodeBehindFields.Keys.ToList());

            var message = $"No element named '{value}' is declared in this document using an x:Name expression.";

            var bundle = new CodeBehindFieldReferenceFixBundle();
            bundle.ReferenceExpression = expression;

            if (!string.IsNullOrEmpty(nearestMatch))
            {
                message += $"\n\nDid you mean {nearestMatch}?";
                bundle.BestMatch = nearestMatch;
            }

             return CreateIssue(message, expression.ParentAttribute, valueSpan, bundle).AsList();
        }
    }
}

