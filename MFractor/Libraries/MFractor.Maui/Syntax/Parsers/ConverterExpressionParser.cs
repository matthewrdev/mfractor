using System;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Xml;

using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Parsers
{
    internal class ConverterExpressionParser : AssignmentExpressionParser<ConverterExpression>
	{
        public override string Identifier => "com.mfractor.parsers.xaml.converter";

        public override string Name => XamlMarkupExtensionNames.XamarinForms.Converter;

        protected override ConverterExpression Create (string keyword, TextSpan keywordSpan, TextSpan expressionSpan, Expression parentExpression, XmlAttribute parentAttribute)
		{
			return new ConverterExpression (keyword, keywordSpan, expressionSpan, parentExpression, parentAttribute);
		}
	}
}

