using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.StaticResources
{
    class StaticResourceStyleTargetTypeIsIncompatible : ExpressionTypeAnalysisRoutine<StaticResourceExpression>
    {
        public override string Documentation => "When a static resource expression returns a style, this code inspection verifies that the TargetType of the given style is compatible with ";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.static_resource_target_type_is_incompatible";

        public override string Name => "Static Resource Style Target Type Is Incompatible";

        public override string DiagnosticId => "MF1077";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(StaticResourceExpression expression,
                                                                  IParsedXamlDocument document,
                                                                  IXamlFeatureContext context)
        {
            var attribute = expression.ParentAttribute;

            var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;

            if (property == null
                || !SymbolHelper.DerivesFrom(property.Type, context.Platform.StackLayout.MetaType)
                || SymbolHelper.DerivesFrom(property.ContainingType, context.Platform.StackLayout.MetaType))
            {
                return null;
            }

            if (!TryGetPreprocessor<StaticResourceAnalysisPreprocessor>(context, out var preprocessor))
            {
                return null;
            }

            var resources = preprocessor.FindNamedStaticResources(expression.Value.Value);

            if (resources == null || !resources.Any())
            {
                return null;
            }

            var result = resources.FirstOrDefault();

            var resourceType = context.Compilation.GetTypeByMetadataName(result.SymbolMetaType);
            if (resourceType == null || !SymbolHelper.DerivesFrom(resourceType, context.Platform.StackLayout.MetaType))
            {
                return null;
            }

            var targetType = context.Compilation.GetTypeByMetadataName(result.TargetType);
            if (targetType == null)
            {
                return null;
            }

            var parentType = context.XamlSemanticModel.GetSymbol(attribute.Parent) as INamedTypeSymbol;
            if (!FormsSymbolHelper.IsTypeMismatch(targetType, parentType, expression.ParentAttribute.Parent, context.Namespaces, context.XmlnsDefinitions, context.Project, context.XamlSemanticModel, context.Platform))
            {
                return null;
            }

            return CreateIssue($"The style provided by this expression targets '{targetType.ToString()}' which is incompatible with the type of this element, " + parentType + ".", expression.ParentAttribute, expression.Span).AsList();
        }
    }
}