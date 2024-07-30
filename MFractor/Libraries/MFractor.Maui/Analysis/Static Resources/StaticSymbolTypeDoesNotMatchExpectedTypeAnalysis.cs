using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.StaticResources
{
    class StaticSymbolTypeDoesNotMatchExpectedTypeAnalysis : ExpressionTypeAnalysisRoutine<StaticBindingExpression>
    {
        public override string Documentation => "Validates that the .NET symbol returned by an `x:Static` expressions matches the expected type for the property.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.static_symbol_type_does_not_match_expected_type";

        public override string Name => "x:Static Return Type Mismatch";

        public override string DiagnosticId => "MF1057";

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        [ImportingConstructor]
        public StaticSymbolTypeDoesNotMatchExpectedTypeAnalysis(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(StaticBindingExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var result = ExpressionEvaluater.EvaluateStaticBindingExpression(context.Project, context.Platform, context.Namespaces, context.XmlnsDefinitions, expression);

            if (result == null
                || result.Symbol == null)
            {
                return null;
            }

            var attr = expression.ParentAttribute;

            var parentSymbol = context.XamlSemanticModel.GetSymbol(attr) as ISymbol;

            if (!(result.Symbol is ISymbol symbol)
                || parentSymbol == null)
            {
                return null;
            }

            if (FormsSymbolHelper.HasTypeConverterAttribute(parentSymbol, context.Platform))
            {
                return null; // Value converter in play, not relevant.
            }

            var returnedType = SymbolHelper.ResolveMemberReturnType(symbol);
            var expectedType = SymbolHelper.ResolveMemberReturnType(parentSymbol);

            if (returnedType == null
                || expectedType == null)
            {
                return null;
            }

            if (!FormsSymbolHelper.IsTypeMismatch(expectedType, returnedType, expression.ParentAttribute.Parent, context.Namespaces, context.XmlnsDefinitions, context.Project, context.XamlSemanticModel, context.Platform))
            {
                return null;
            }

            return CreateIssue($"This x:Static expression returns a '{returnedType}' but '{attr.Name.LocalName}' expects a '{expectedType}' typed value.", expression.ParentAttribute, expression.Span).AsList();
        }
    }
}

