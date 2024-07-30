using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class PathExpressionParser : ExpressionParser
	{
        public override string Identifier => "com.mfractor.parsers.xaml.path";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.Path;

        public override Expression Parse(string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
			ExpressionComponent name, value;
            if (!ExpressionParserHelper.ExtractAssignmentExpressionComponents(expression, expressionStart, out name, out value))
            {
                return null;
            }

            Expression childExpression;
            if (ExpressionParserHelper.IsPropertyAssignmentExpression(value.Content))
            {
                var parser = ParserProvider.ResolveParser(value.Content, project, namespaces, xmlnsDefinitions, platform);
                childExpression = parser.Parse(value.Content, value.Span, parentExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);
            }
            else
            {
                childExpression = new LiteralValueExpression(value, parentExpression, parentAttribute);
            }

            if (childExpression == null)
			{
				return null;
			}

            var path = new PathExpression(name.Content, name.Span, TextSpan.FromBounds(expressionStart, expressionEnd), parentExpression, parentAttribute);
			path.Children.Add(childExpression);

			return path;
		}
	}
}

