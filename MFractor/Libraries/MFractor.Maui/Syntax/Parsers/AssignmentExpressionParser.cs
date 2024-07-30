using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    internal abstract class AssignmentExpressionParser<TExpression> : ExpressionParser where TExpression : PropertyAssignmentExpression
	{
		public override Expression Parse (string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
            if (!ExpressionParserHelper.ExtractAssignmentExpressionComponents(expression, expressionStart, out var keyword, out var value))
            {
                return null;
            }

            var childExpression = ParseValue(value, parentExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);


            if (childExpression == null) {
				return null;
			}

            var path = Create (keyword.Content, keyword.Span, TextSpan.FromBounds(expressionStart, expressionEnd), parentExpression, parentAttribute);
			path.Children.Add (childExpression);

			return path;
		}

		protected abstract TExpression Create (string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute);

		protected virtual Expression ParseValue(ExpressionComponent value, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
			Expression childExpression = null;
			if (ExpressionParserHelper.IsExpression (value.Content)) {
                var parser = ParserProvider.ResolveParser (value.Content, project, namespaces, xmlnsDefinitions, platform);
				childExpression = parser.Parse (value.Content, value.Span, parentExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);
			} else {
				childExpression = new LiteralValueExpression (value, parentExpression, parentAttribute);
			}

			return childExpression;
		}
	}
}

