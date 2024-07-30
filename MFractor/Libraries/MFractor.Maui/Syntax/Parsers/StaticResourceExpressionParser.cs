using System.Linq;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class StaticResourceExpressionParser : MarkupExtensionExpressionParser<StaticResourceExpression>
	{
        public override string Identifier => "com.mfractor.parsers.xaml.static_resource";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.StaticResource;

        protected override StaticResourceExpression Create (string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
		{
			return new StaticResourceExpression (keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
        }

        protected override IXamlNamespace GetXamlNamespace(IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            return namespaces.ResolveNamespaceForSchema(platform.SchemaUrl);
        }
    }
}

