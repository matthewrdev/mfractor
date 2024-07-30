using System.Linq;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;

using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class BindingExpressionParser : MarkupExtensionExpressionParser<BindingExpression>
	{
        public override string Identifier => "com.mfractor.parsers.xaml.binding";

        public override string Name => "Binding";

        protected override IXamlNamespace GetXamlNamespace(IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            return namespaces.ResolveNamespaceForSchema(platform.SchemaUrl);
        }

        protected override BindingExpression Create (string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
		{
			return new BindingExpression (keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
		}
	}
}

