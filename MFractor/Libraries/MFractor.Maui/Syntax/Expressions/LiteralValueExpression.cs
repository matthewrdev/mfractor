using System;

using MFractor.Maui.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class LiteralValueExpression : ValueExpression
	{
		public LiteralValueExpression(string value, 
		                              TextSpan span, 
		                              Expression parentExpression,
									  XmlAttribute parentAttribute)
			: base(span, parentExpression, parentAttribute)
		{
			this.Value = value;
			this.IsMalformed = false;
		}
		
		public LiteralValueExpression (ExpressionComponent expressionComponent, Expression parentExpression,
									  XmlAttribute parentAttribute)
            : this(expressionComponent.Content, expressionComponent.Span, parentExpression, parentAttribute)
		{
		}

		public override string ToString()
		{
			return Value;
		}
	}
}

