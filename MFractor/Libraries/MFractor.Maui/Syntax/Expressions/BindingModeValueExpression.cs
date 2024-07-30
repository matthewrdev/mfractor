using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public enum BindingMode
	{
		Default,
		TwoWay,
		OneWay,
		OneWayToSource,

		Unknown,
	}

	public class BindingModeValueExpression : ValueExpression
	{
		public BindingModeValueExpression(string bindingModeValue,
                                          TextSpan span, 
                                          Expression parentExpression,
                                          XmlAttribute parentAttribute)
			: base(span, parentExpression, parentAttribute)
		{
			this.Value = bindingModeValue;

			var mode = BindingMode.Unknown;
			if (Enum.TryParse(bindingModeValue, out mode))
			{
				BindingMode = mode;
				IsMalformed = false;
			}
			else {
				BindingMode = BindingMode.Unknown;
				IsMalformed = true;
			}
		}

		public BindingMode BindingMode { get; set; }

		public override string ToString()
		{
			return BindingMode.ToString();
		}
	}
}

