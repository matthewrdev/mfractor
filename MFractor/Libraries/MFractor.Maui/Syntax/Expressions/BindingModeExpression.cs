using System;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class BindingModeExpression : PropertyAssignmentExpression
	{
		public BindingModeExpression(string name,
									  TextSpan nameSpan,
									  TextSpan expressionSpan,
									  Expression parentExpression,
									  XmlAttribute parentAttribute)
			: base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
		{
		}

		public bool HasBindingMode
		{
			get
			{
				return HasChildExpression<BindingModeValueExpression > ();
			}
		}

		public BindingModeValueExpression BindingModeValue
		{
			get
			{
				return GetChildExpression<BindingModeValueExpression>();
			}
		}

		public override string ToString()
		{
			string mode = HasBindingMode ? BindingModeValue.ToString() : "";

			return $"{PropertyName}={mode}";
		}
	}
}

