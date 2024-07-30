using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.DataBinding
{
    class BindingExpressionIsAgainstNonPublicProperty : ExpressionTypeAnalysisRoutine<BindingExpression>
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.binding_expression_is_against_nonpublic_property";

        public override string Name => "Binding Against Non-Public Property";

        public override string Documentation => "Inspects data-binding expressions and validates that the property return is a public property. Data-binding against a non-public property causes data-binding to fail.";

        public override string DiagnosticId => "MF1008";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(BindingExpression expression, 
                                                                  IParsedXamlDocument document,
                                                                  IXamlFeatureContext context)
        {
            var bindingContext = context.XamlSemanticModel.GetBindingContext(expression, context);
            if (bindingContext == null)
            {
                return null;
            }

            if (!expression.HasReferencedSymbol)
            {
                return null; // Can't evalaute.
            }

            var symbol = context.XamlSemanticModel.GetDataBindingExpressionResult(expression, context) as ISymbol;
            if (symbol == null)
            {
                return null;
            }

            if (expression.ReferencesBindingContext && symbol.Equals(bindingContext))
            {
                return null;
            }

            var property = symbol as IPropertySymbol;
            if (property == null || property.DeclaredAccessibility == Accessibility.Public)
            {
                return null;
            }

            var accesibility = property.DeclaredAccessibility.ToString();

            var summary = accesibility.SeparateUpperLettersBySpace().ToLower();

            var message = $"This binding does not return a public property therefore data-binding will not take effect.\n\n'{symbol.Name}' from '{bindingContext}' has the following accessibility modifiers: " + summary + ".";

            return CreateIssue(message, expression.ParentAttribute, expression.Span).AsList();
        }
    }
}
