using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class BindingModeExpressionParser : AssignmentExpressionParser<BindingModeExpression>
	{
        public override string Identifier => "com.mfractor.parsers.xaml.binding_mode";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.BindingMode;

        protected override BindingModeExpression Create (string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
		{
			return new BindingModeExpression (keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
		}

		protected override Expression ParseValue (ExpressionComponent value, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
            return new BindingModeValueExpression (value.Content, value.Span, parentExpression, parentAttribute);
		}
	}
}

