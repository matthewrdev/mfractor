using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class StylePropertySetterDoesNotExistAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "This code inspection looks at `Setter` elements when they are used inside a `Style` and validates that the member specified in the `Property` attribute exists on the type symbol referenced in the parent `Style`s `TargetType` attribute.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.style_property_setter_does_not_exist";

        public override string Name => "Property Setter Does Not Exist In Style TargetType";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1061";

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        public IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        [ImportingConstructor]
        public StylePropertySetterDoesNotExistAnalysis(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != "Property"
                && syntax.HasValue)
            {
                return null;
            }

            var value = syntax.Value.Value;
            if (XamlSyntaxHelper.IsPropertySetter(value))
            {
                return null;
            }

            var outerParentSyntax = syntax.Parent.Parent;
            if (outerParentSyntax == null)
            {
                return null;
            }

            var parentType = context.XamlSemanticModel.GetSymbol(syntax.Parent) as ITypeSymbol;
            var outerParentType = context.XamlSemanticModel.GetSymbol(outerParentSyntax) as ITypeSymbol;

            if (parentType == null || outerParentType == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(parentType, context.Platform.Setter.MetaType)
                || !SymbolHelper.DerivesFrom(outerParentType, context.Platform.Style.MetaType))
            {
                return null;
            }

            var targetTypeAttr = outerParentSyntax.GetAttribute((attr) => attr.Name.LocalName == "TargetType");
            if (targetTypeAttr == null)
            {
                return null;
            }

            var symbolExpression = context.XamlSemanticModel.GetExpression(syntax) as DotNetTypeSymbolExpression;
            if (symbolExpression == null)
            {
                return null;
            }

            var evalResult = ExpressionEvaluater.EvaluateDotNetSymbolExpression(context.Project, context.Platform, context.Namespaces, context.XmlnsDefinitions, symbolExpression);

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

            var message = $"'{syntax.Value}' does not exist in `{targetTypeAttr.Value}.";
            if (property != null)
            {
                message += $"\n\nDid you mean '{property.Name}'?";
            }

            // Find the nearest and pass to fix.
            return CreateIssue(message, syntax, syntax.Value.Span, property).AsList();
        }
    }
}

