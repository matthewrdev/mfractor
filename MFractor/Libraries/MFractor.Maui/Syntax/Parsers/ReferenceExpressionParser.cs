using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;

namespace MFractor.Maui.Syntax.Parsers
{
    class ReferenceExpressionParser : MarkupExtensionExpressionParser<ReferenceExpression>
	{
        public override string Identifier => "com.mfractor.parsers.xaml.reference";

        public override string Name => XamlMarkupExtensionNames.Microsoft.Reference;

        protected override IXamlNamespace GetXamlNamespace(IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            return namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);
        }

        protected override ReferenceExpression Create(string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
		{
			return new ReferenceExpression(keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
		}
    }
}

