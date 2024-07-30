using System;
using MFractor.IOC;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    public abstract class ExpressionParser : IExpressionParser
	{
		readonly Lazy<IExpressionParserRepository> expressionParserRepository = new Lazy<IExpressionParserRepository>(() => Resolver.Resolve<IExpressionParserRepository>());
		public IExpressionParserRepository ParserProvider => expressionParserRepository.Value;

		public abstract string Identifier { get; }

		public abstract string Name { get; }

        public virtual int ParserPriority => 0;

		public virtual bool CanParse(XmlAttribute attribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
            if (!attribute.HasValue)
            {
                return false;
            }

            return CanParse(attribute.Value.Value, project, namespaces, xmlnsDefinitions, platform);
		}

        public virtual bool CanParse (string expression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
			return ExpressionParserHelper.BeginsWithKeyword (expression, this.Name);
		}

		public Expression Parse (XmlAttribute attribute, Expression parentExpression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
            var expression = Parse (attribute.Value?.Value, attribute.Value.Span, parentExpression, attribute, project, namespaces, xmlnsDefinitions, platform);

			if (expression != null) {
				expression.ParentAttribute = attribute;
			}

			return expression;
		}
		public Expression Parse(string expression, TextSpan span, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
            return Parse(expression, span.Start, span.End, parentExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);
		}

		public Expression Parse(ExpressionComponent expression, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
            return Parse(expression.Content, expression.Span, parentExpression, parentAttribute, project, namespaces, xmlnsDefinitions, platform);
		}

		public abstract Expression Parse (string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform);
	}
}

