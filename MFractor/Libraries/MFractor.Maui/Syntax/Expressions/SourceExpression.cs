using System;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class SourceExpression : PropertyAssignmentExpression
	{
		public SourceExpression (string name,
								 TextSpan nameSpan,
								 TextSpan expressionSpan,
								 Expression parentExpression,
								 XmlAttribute parentAttribute)
			: base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
		{
		}

        public bool IsReferenceValue => HasChildExpression<ReferenceExpression>();

        public ReferenceExpression Reference => GetChildExpression<ReferenceExpression>();

        public LiteralValueExpression SourceValue => GetChildExpression<LiteralValueExpression>();

        public override string ToString()
		{
			var content = PropertyName + "=";

			if (Reference != null && Reference.HasReferencedXName)
			{
				content += "{" + Reference.ToString() + "}";
			}
			else if (SourceValue != null && SourceValue.HasValue)
			{
				content += SourceValue.Value;
			}

			return content;
		}
	}
}

