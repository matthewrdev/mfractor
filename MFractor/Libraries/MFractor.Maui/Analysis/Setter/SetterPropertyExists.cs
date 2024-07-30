using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Setter
{
    class SetterPropertyExists : XamlCodeAnalyser
    {
        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.setter_property_does_not_exist";

        public override string Name => "Setter Property Exists";

        public override string Documentation => "Inspects the `Property` attribute for a `.Setter` and validates that it exists in the parents specified `TargetType`.";

        public override string DiagnosticId => "MF1049";

        [ImportingConstructor]
        public SetterPropertyExists(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, 
                                                           IParsedXamlDocument document, 
                                                           IXamlFeatureContext context)
        {
            if (syntax.Name.FullName != "Property" 
                || !syntax.HasValue)
            {
                return null;
            }

            var propertyValue = syntax.Value.Value;
            if (XamlSyntaxHelper.ExplodeAttachedProperty(propertyValue, out var className, out var propertyName))
            {
                return null;
            }

            var type = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(type, context.Platform.Setter.MetaType))
            {
                return null;
            }

            var outerParent = syntax.Parent?.Parent;
            var outerType = context.XamlSemanticModel.GetSymbol(outerParent) as INamedTypeSymbol;
            if (outerType == null)
            {
                return null;
            }

            var targetType = outerParent.GetAttributeByName("TargetType");
            if (targetType == null
                || !targetType.HasValue)
            {
                return null;
            }

            var symbolExpression = context.XamlSemanticModel.GetExpression(targetType) as DotNetTypeSymbolExpression;
            if (symbolExpression == null)
            {
                return null;
            }

            var evalResult = ExpressionEvaluater.EvaluateDotNetSymbolExpression(context.Project,
                                                                                context.Platform,
                                                                                context.Namespaces,
                                                                                context.XmlnsDefinitions,
                                                                                symbolExpression);

            var typeSymbol = evalResult?.GetSymbol<INamedTypeSymbol>();
            if (typeSymbol == null)
            {
                return null;
            }

            var property = SymbolHelper.ResolveNearestNamedMember(typeSymbol, syntax.Value.Value);
            if (property != null
                && property.Name == syntax.Value.Value)
            {
                return null;
            }

            var message = $"'{syntax.Value}' does not exist in `{targetType.Value}";
            if (property != null)
            {
                message += $"\n\nDid you mean '{property.Name}'?";
            }

            return CreateIssue(message, syntax, syntax.Value.Span, property).AsList();
        }
    }
}
