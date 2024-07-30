using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class ConverterInputTypeMismatchAnalysis : ExpressionTreeAnalysisRoutine<ConverterExpression>
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        public override string Documentation => "Inspects the `Converter` property of a `Binding `expression and validates that the input type is correct. This analyser requires that the `IValueConverter` implementation uses the [`ValueConversion`](/xamarin-forms/value-converter-type-safety.md) attribute to declare it input type.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.converter_input_type_mismatch";

        public override string Name => "Value Converter Input Type Mismatch";

        public override string DiagnosticId => "MF1016";

        [ImportingConstructor]
        public ConverterInputTypeMismatchAnalysis(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(ConverterExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!expression.HasChildren
                || expression.AssignmentValue == null)
            {
                return null;
            }

            var result = ExpressionEvaluater.Evaluate(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces,  expression.AssignmentValue);

            if (result == null
                || result.Symbol as INamedTypeSymbol == null)
            {
                return null;
            }

            var converter = result.Symbol as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(converter, context.Platform.ValueConverter.MetaType))
            {
                return null;
            }

            if (!FormsSymbolHelper.ResolveValueConverterConstraints(converter, out var inputType, out var outputType, out var paramType))
            {
                return null;
            }

            var binding = expression.ParentExpression as BindingExpression;
            if (binding == null)
            {
                return null;
            }

            var resolvedType = ExpressionEvaluater.EvaluateDataBindingExpression(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, binding);

            if (resolvedType == null || resolvedType.Symbol as ISymbol == null)
            {
                return null;
            }

            var bindingResultType = binding.ReferencesBindingContext ? resolvedType.Symbol as ITypeSymbol : SymbolHelper.ResolveMemberReturnType(resolvedType.Symbol as ISymbol);
            if (!SymbolHelper.IsTypeMismatch(bindingResultType, inputType))
            {
                return null;
            }

            return CreateIssue($"{converter.Name} expects a {inputType} but the binding value '{binding.ReferencedSymbolValue}' returns a {bindingResultType.ToString()}", expression.ParentAttribute, expression.Span).AsList();
        }

        public override bool ShouldInspectExpressionChildren(Expression expression, int expressionDepth)
        {
            if (expression is BindingExpression)
            {
                return true;
            }

            return false;
        }
    }
}
