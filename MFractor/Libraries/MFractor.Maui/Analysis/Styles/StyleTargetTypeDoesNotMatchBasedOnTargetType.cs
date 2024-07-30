using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Styles
{
    class StyleTargetTypeDoesNotMatchBasedOnTargetType : XamlCodeAnalyser
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.style_target_type_does_not_match_basedon_target_type";

        public override string Name => "Style TargetType Does Not Match BasedOn TargetType";

        public override string Documentation => "Inspects XAML styles that use the `BasedOn` property to inherit from another style and validates the `TargetType` of the current style matches the `TargetType` defined by the `BasedOn` style.";

        public override string DiagnosticId => "MF1062";

        [ImportingConstructor]
        public StyleTargetTypeDoesNotMatchBasedOnTargetType(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax,
                                                           IParsedXamlDocument document,
                                                           IXamlFeatureContext context)
        {
            var type = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(type, context.Platform.Style.MetaType))
            {
                return null;
            }

            if (syntax.Name.FullName != "BasedOn"
                || !syntax.HasValue)
            {
                return null;
            }

            var targetTypeAttr = syntax.Parent.GetAttributeByName("TargetType");
            if (targetTypeAttr == null
                || !targetTypeAttr.HasValue)
            {
                return null;
            }

            var targetTypeExpression = context.XamlSemanticModel.GetExpression(syntax) as DotNetTypeSymbolExpression;
            if (targetTypeExpression == null)
            {
                return null;
            }

            var targetType = ExpressionEvaluater.EvaluateDotNetSymbolExpression(context.Project, context.Platform, context.Namespaces, context.XmlnsDefinitions, targetTypeExpression)?.GetSymbol<INamedTypeSymbol>();
            if (targetType == null)
            {
                return null;
            }

            var resourceExpression = context.XamlSemanticModel.GetExpression(syntax) as StaticResourceExpression;
            if (resourceExpression == null
                || resourceExpression.Value == null
                || !resourceExpression.Value.HasValue)
            {
                return null;
            }

            if (!TryGetPreprocessor(context, out StaticResourceAnalysisPreprocessor preprocessor))
            {
                return null;
            }

            var staticResources = preprocessor.FindNamedStaticResources(resourceExpression.Value.Value);

            if (staticResources == null
                || !staticResources.Any())
            {
                return null;
            }

            var resource = staticResources.First();

            if (resource.TargetType == targetType.ToString())
            {
                return null;
            }

            return CreateIssue($"This resource's target type is '{targetType}' however it's based on {resource.Name}, which has a target type of '{resource.TargetType}'. This will cause a runtime crash.", syntax, syntax.Span).AsList();
        }
    }
}
