using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Triggers
{
    class TriggerTargetTypeDoesNotMatchParent : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.trigger_target_type_does_not_match_parent";

        public override string Name => "Trigger TargetType Does Not Match Parent";

        public override string Documentation => "Inspects usages of the `TargetType` property for a `.TriggerBase` and validates that the type provided matches the outer XAML node.";

        public override string DiagnosticId => "MF1065";

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        [ImportingConstructor]
        public TriggerTargetTypeDoesNotMatchParent(Lazy<IMarkupExpressionEvaluater> expressionEvaluater,
                                                   Lazy<IXamlTypeResolver> xamlTypeResolver)
        {
            this.expressionEvaluater = expressionEvaluater;
            this.xamlTypeResolver = xamlTypeResolver;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.FullName != "TargetType")
            {
                return null;
            }

            var parentType = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(parentType, context.Platform.TriggerBase.MetaType))
            {
                return null;
            }

            if (syntax.Parent.Parent == null
                || !XamlSyntaxHelper.ExplodePropertySetter(syntax.Parent.Parent.Name.FullName, out var className, out var propName)
               || propName != "Triggers")
            {
                return null;
            }

            var outerParent = syntax.Parent.Parent?.Parent;
            var targetType = context.XamlSemanticModel.GetSymbol(outerParent) as INamedTypeSymbol;


            if (SymbolHelper.DerivesFrom(targetType, context.Platform.Style.MetaType))
            {
                var targetTypeAttribute = outerParent.GetAttributeByName("TargetType");

                if (targetTypeAttribute == null || !targetTypeAttribute.HasValue)
                {
                    return null;
                }

                var innerValue = targetTypeAttribute.Value.Value;

                if (!XamlSyntaxHelper.ExplodeTypeReference(innerValue, out var xmlns, out var @class))
                {
                    return null;
                }

                var @namespace = context.Namespaces.ResolveNamespace(xmlns);
                if (@namespace == null)
                {
                    return null;
                }

                targetType = XamlTypeResolver.ResolveType(@class, @namespace, context.Project, context.XmlnsDefinitions);
            }

            if (targetType == null)
            {
                return null;
            }

            if (!syntax.HasValue)
            {
                return CreateIssue("No TargetType is defined for this trigger. This will cause a runtime crash", syntax, syntax.Span).AsList();
            }

            var symbolExpression = context.XamlSemanticModel.GetExpression(syntax) as DotNetTypeSymbolExpression;
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

            if (typeSymbol == targetType)
            {
                return null;
            }

            return CreateIssue($"The trigger target type, {typeSymbol}, does not match the parent type, {targetType}. This may cause a runtime crash.", syntax, syntax.Value.Span).AsList();
        }
    }
}
