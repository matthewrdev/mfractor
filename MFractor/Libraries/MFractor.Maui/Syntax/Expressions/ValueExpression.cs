using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class ValueExpression : Expression
	{
		public ValueExpression(TextSpan valueSpan,
                               Expression parentExpression,
                               XmlAttribute parentAttribute)
            : base(valueSpan, parentExpression, parentAttribute)
		{
		}

		public string Value { get; set; }

        public bool HasValue => !string.IsNullOrEmpty(Value);
    }
}

