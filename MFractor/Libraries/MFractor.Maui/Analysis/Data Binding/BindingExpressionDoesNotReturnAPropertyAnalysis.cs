using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.DataBinding
{
    class BindingExpressionDoesNotReturnAPropertyAnalysis : ExpressionTypeAnalysisRoutine<BindingExpression>
    {
        public override string Documentation => "Evaluates a `Binding` expression and validates that it points to property within the binding context. This analyser requires an explict or implicit binding context.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.binding_expression_does_not_return_a_property";

        public override string Name => "Validate Binding Expressions Return A Property";

        public override string DiagnosticId => "MF1007";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(BindingExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
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

            if (expression.ReferencesBindingContext && SymbolEqualityComparer.Default.Equals(symbol, bindingContext))
            {
                return null;
            }

            var property = symbol as IPropertySymbol;
            if (property != null)
            {
                return null;
            }

            var message = $"'{symbol.Name}' is not a property; bindings must return a property rather than a field or method.";

            var suggestedProperty = SymbolHelper.ResolveNearestNamedProperty(bindingContext, symbol.Name);
            if (suggestedProperty != null)
            {
                message += $"\n\nDid you mean '{suggestedProperty.Name}'?";
            }

            var content = new BindingExpressionPropertyBundle()
            {
                Expression = expression,
                PropertySuggestion = suggestedProperty,
                Property = expression.ReferencedSymbolValue,
            };

            return CreateIssue(message, expression.ParentAttribute, expression.ReferencedSymbolSpan, content).AsList();
        }
    }
}

