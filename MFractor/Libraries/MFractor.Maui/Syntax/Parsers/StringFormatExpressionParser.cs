using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class StringFormatExpressionParser : ExpressionParser
	{
        public override string Identifier => "com.mfractor.parsers.xaml.string_format";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.StringFormat;

        public override Expression Parse (string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
			// Resolve the value expression
			ExpressionComponent name, value;
			if (!ExpressionParserHelper.ExtractAssignmentExpressionComponents (expression, expressionStart, out name, out value)) {
				return null;
			}

            var stringFormat = new StringFormatExpression (name.Content, name.Span, TextSpan.FromBounds(expressionStart, expressionEnd), parentExpression, parentAttribute);
            stringFormat.Children.Add(new StringFormatValueExpression(value.Content, value.Span, parentExpression, parentAttribute));
			
			return stringFormat;
		}
	}
}

