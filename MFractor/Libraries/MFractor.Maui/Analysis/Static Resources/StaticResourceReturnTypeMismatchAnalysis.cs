using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.StaticResources
{
    class StaticResourceReturnTypeMismatchAnalysis : ExpressionTypeAnalysisRoutine<StaticResourceExpression>
    {
        public override string Documentation => "Validates that the symbol returned by a `StaticResource` expression matches the expected type for the property.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.static_resource_return_type_mismatch";

        public override string Name => "StaticResource Return Type Mismatch";

        public override string DiagnosticId => "MF1056";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(StaticResourceExpression expression,
                                                                    IParsedXamlDocument document,
                                                                    IXamlFeatureContext context)
        {
            if (!TryGetPreprocessor<StaticResourceAnalysisPreprocessor>(context, out var preprocessor))
            {
                return null;
            }

            var result = preprocessor.FindNamedStaticResources(expression.Value.Value)?.FirstOrDefault();
            if (result == null || result.ReturnType is null)
            {
                return null;
            }

            var attr = expression.ParentAttribute;

            var parentSymbol = context.XamlSemanticModel.GetSymbol(attr);

            if (FormsSymbolHelper.HasTypeConverterAttribute(parentSymbol, context.Platform))
            {
                return null; // Value converter in play, not relevant.
            }

            var symbol = context.Compilation.GetTypeByMetadataName(result.ReturnType);
            var expectedType = SymbolHelper.ResolveMemberReturnType(parentSymbol);

            if (!(symbol is ITypeSymbol returnedType)
                || expectedType == null)
            {
                return null;
            }

            if (!FormsSymbolHelper.IsTypeMismatch(expectedType,
                returnedType,
                expression.ParentAttribute.Parent,
                context.Namespaces,
                context.XmlnsDefinitions,
                context.Project,
                context.XamlSemanticModel,
                context.Platform))
            {
                return null;
            }

            return CreateIssue($"This expression returns a '{returnedType}' but '{attr.Name.LocalName}' expects a '{expectedType}' typed value.", expression.ParentAttribute, expression.Span).AsList();
        }
    }
}
