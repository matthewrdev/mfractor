using System;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class StaticBindingExpressionParser : MarkupExtensionExpressionParser<StaticBindingExpression>
	{
        public override string Identifier => "com.mfractor.parsers.xaml.static_binding";

        public override string Name => XamlMarkupExtensionNames.Microsoft.StaticBinding;


        protected override StaticBindingExpression Create (string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
		{
			return new StaticBindingExpression (keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
		}

		protected override ValueExpression CreateValueExpression(ExpressionComponent value, Expression parentExpression, XmlAttribute parentAttribute)
		{
            return new DotNetTypeSymbolExpression(value.Content, value.Span, parentExpression, parentAttribute);
		}

        protected override IXamlNamespace GetXamlNamespace(IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            return namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);
        }
    }
}

