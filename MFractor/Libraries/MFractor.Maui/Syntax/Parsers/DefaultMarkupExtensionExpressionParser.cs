using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    /// <summary>
    /// This expression parser is the fallback parser that deconstructs an expression into its child components.
    /// </summary>
    class DefaultMarkupExtensionExpressionParser : ExpressionParser
    {
        public override string Identifier => "com.mfractor.parsers.xaml.default";

        public override string Name => XamlMarkupExtensionNames.Default;

        public override int ParserPriority => int.MaxValue;

        public override bool CanParse(string expression, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            return ExpressionParserHelper.IsExpression(expression);
        }

        public override Expression Parse(string expression, int expressionStart, int expressionEnd, Expression parentExpression, XmlAttribute parentAttribute, Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, IXamlPlatform platform)
        {
            var expressionSpan = TextSpan.FromBounds(expressionStart, expressionEnd);
            if (!ExpressionParserHelper.ResolveMarkupExtensionExpressionNameAndRegions(expression, expressionStart, out var fullNameSpan, out var fullName, out _, out _, out _, out _))
            {
                return null;
            }

            var unknownExpression = new DefaultMarkupExtensionExpression(fullName, fullNameSpan, expressionSpan, parentExpression, parentAttribute);

            unknownExpression.Span = TextSpan.FromBounds(expressionStart, expressionEnd);
            unknownExpression.IsMalformed = false;

            return unknownExpression;
        }
    }
}

