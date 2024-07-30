using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class DynamicResourceExpressionParser : ExpressionParser
	{
        public override string Identifier => "com.mfractor.parsers.xaml.dynamic_resource";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.DynamicResource;

        public override Expression Parse(string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
			// Resolve the value expression
			ExpressionComponent keyword, value;
			if (!ExpressionParserHelper.ExtractAssignmentExpressionComponents(expression, expressionStart, out keyword, out value))
			{
				return null;
			}

            var resource = new DynamicResourceExpression(keyword.Content, keyword.Span, TextSpan.FromBounds(expressionStart, expressionEnd), parentExpression, parentAttribute);
            resource.Children.Add(new LiteralValueExpression(value.Content, value.Span, parentExpression, parentAttribute));

			return resource;
		}
	}
}

