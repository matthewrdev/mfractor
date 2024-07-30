using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Styles;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Styles
{
    class PropertyValueIsAlreadyAppliedByStyle : XamlCodeAnalyser
    {
        public override string Documentation => "When a `Style` is applied  within XAML, it should always specify a type it targets using the `TargetType` property. This analysis check inspects for usages of `Style` that don't assign the `TargetType` property.";

        public override IssueClassification Classification => IssueClassification.Improvement;

        public override string Identifier => "com.mfractor.code.analysis.xaml.property_value_is_already_applied_by_style";

        public override string Name => "Property Value Is Already Applied By Style";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1081";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;
            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.VisualElement.MetaType))
            {
                return default;
            }

            if (!syntax.HasAttributes)
            {
                return default;
            }

            var styleAttribute = syntax.GetAttributeByName("Style");
            if (styleAttribute == null)
            {
                return default;
            }

            var expression = context.XamlSemanticModel.GetExpression(styleAttribute) as StaticResourceExpression;
            if (expression == null || !expression.Value.HasValue)
            {
                return default;
            }

            if (!TryGetPreprocessor(context, out StyleAnalysisPreprocessor preprocessor))
            {
                return default;
            }

            var style = preprocessor.GetNamedStyle(expression.Value.Value, context.Document.FilePath, context.Project, context.Platform);
            if (style == null)
            {
                return default;
            }

            var issues = new List<ICodeIssue>();

            foreach (var property in style.Properties.Properties)
            {
                var attribute = syntax.GetAttributeByName(property.Name);

                if (attribute != null && attribute.HasValue)
                {
                    var value = attribute.Value.Value;

                    var propertyValue = property.Value;

                    if (propertyValue is LiteralStylePropertyValue literalValue
                        && literalValue.Value == value)
                    {
                        var bundle = new PropertyValueIsAlreadyAppliedByStyleBundle(style, property);

                        var issue = CreateIssue($"This attribute initialisation is redundant as the style applied to this element, {style.Name}, already sets {property.Name} to '{value}'.", attribute, attribute.Span, bundle);

                        issues.Add(issue);
                    }
                }
            }

            return issues;
        }
    }
}
