using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class NameExpressionParser : AssignmentExpressionParser<NameExpression>
	{
        public override string Identifier => "com.mfractor.parsers.xaml.name";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.Name;

        protected override NameExpression Create (string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
		{
			return new NameExpression (keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
		}

		protected override Expression ParseValue (ExpressionComponent value, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
		{
			return new LiteralValueExpression (value, parentExpression, parentAttribute);
		}
	}
}

