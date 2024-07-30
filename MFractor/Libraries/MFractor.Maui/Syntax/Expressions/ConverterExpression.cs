using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class ConverterExpression : PropertyAssignmentExpression
	{
		public ConverterExpression (string name,
								  TextSpan nameSpan,
								  TextSpan expressionSpan,
								  Expression parentExpression,
								  XmlAttribute parentAttribute)
			: base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
		{
		}
	}
}

