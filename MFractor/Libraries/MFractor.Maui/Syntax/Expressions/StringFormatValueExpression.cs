using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class StringFormatValueExpression : ValueExpression
	{
		public StringFormatValueExpression(string formatContent, 
		                                   TextSpan formatSpan, 
		                                   Expression parentExpression,
		                                   XmlAttribute parentAttribute)
			: base(formatSpan, parentExpression, parentAttribute)
		{
			Value = formatContent;
			Span = formatSpan;

            TextSpan span;
			bool malformed = false;
			FormattedValue = ExpressionParserHelper.ExtractStringFormatValue (formatContent, formatSpan, out span, out malformed);
            FormattedValueSpan = TextSpan.FromBounds(span.Start + 1, span.End - 1);
			IsMalformed = malformed;
		}

		public string FormattedValue { get; set; }

        public TextSpan FormattedValueSpan { get; set; }

		public override string ToString()
		{
			return $"'{FormattedValue}'";
		}
	}
}

