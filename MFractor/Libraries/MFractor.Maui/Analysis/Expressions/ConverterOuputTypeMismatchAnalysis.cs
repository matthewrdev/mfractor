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
    class ConverterOuputTypeMismatchAnalysis : ExpressionTreeAnalysisRoutine<ConverterExpression>
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        public override string Documentation => "Inspects the `Converter` component of a `Binding` expression and validates that the returned output type is valid for the parent attribute. This analyser requires that the `IValueConverter` implementation uses the [`ValueConversion`](/xamarin-forms/value-converter-type-safety.md) attribute to declare it input type.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.converter_output_type_mismatch";

        public override string Name => "Value Converter Output Type Mismatch";

        public override string DiagnosticId => "MF1017";


        [ImportingConstructor]
        public ConverterOuputTypeMismatchAnalysis(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
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

            var parentSymbol = context.XamlSemanticModel.GetSymbol(expression.ParentAttribute) as ISymbol;
            if (parentSymbol == null)
            {
                return null;
            }

            INamedTypeSymbol parentType;
            parentType = SymbolHelper.ResolveMemberReturnType(parentSymbol) as INamedTypeSymbol;
            if (parentType == null)
            {
                return null;
            }

            var result = ExpressionEvaluater.Evaluate(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, expression.AssignmentValue);

            if (result == null
                || result.Symbol as INamedTypeSymbol == null)
            {
                return null;
            }

            var converter = result.Symbol as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(converter,  context.Platform.ValueConverter.MetaType))
            {
                return null;
            }

            ITypeSymbol inputType;
            ITypeSymbol outputType;
            ITypeSymbol paramType;
            if (!FormsSymbolHelper.ResolveValueConverterConstraints(converter, out inputType, out outputType, out paramType))
            {
                return null;
            }

            if (!SymbolHelper.IsTypeMismatch(outputType, parentType))
            {
                return null;
            }

            return CreateIssue($"{converter.Name} returns a {outputType} but {expression.ParentAttribute.Name} expects a {parentType.ToString()}", expression.ParentAttribute, expression.Span).AsList();
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
