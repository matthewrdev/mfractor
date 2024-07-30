using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class SourceExpressionParser : ExpressionParser
	{
        public override string Identifier => "com.mfractor.parsers.xaml.source";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.Source;

        public override Expression Parse(string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
			ExpressionComponent keyword, value;
			Expression childExpression = null;
			if (!ExpressionParserHelper.ExtractAssignmentExpressionComponents(expression, expressionStart, out keyword, out value))
			{
				return null;
			}

			if (ExpressionParserHelper.IsExpression(value.Content))
			{
				var parser = ParserProvider.ResolveParser(value.Content, project, namespaces, xmlnsDefinitions, platform);
                childExpression = parser.Parse(value.Content, value.Span, parentExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);
			}
			else {
				childExpression = new LiteralValueExpression(value, parentExpression, parentAttribute);
			}

			if (childExpression == null)
			{
				return null;
			}

            var sourceExpression = new SourceExpression(keyword.Content, keyword.Span, TextSpan.FromBounds(expressionStart, expressionEnd), parentExpression, parentAttribute);
			sourceExpression.Children.Add(childExpression);

			return sourceExpression;
		}
	}
}

