using MFractor.Maui.Syntax.Expressions;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    class ConverterParameterExpressionParser : AssignmentExpressionParser<ConverterParameterExpression>
    {
        public override string Identifier => "com.mfractor.parsers.xaml.converter_parameter";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.ConverterParameter;

        protected override ConverterParameterExpression Create(string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
        {
            return new ConverterParameterExpression(keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
        }
    }
}

