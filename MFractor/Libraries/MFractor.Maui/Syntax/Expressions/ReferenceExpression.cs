using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class ReferenceExpression : MarkupExtensionExpression
	{
        public ReferenceExpression(string name,
                                   TextSpan nameSpan,
                                   TextSpan expressionSpan,
                                      Expression parentExpression,
                                         XmlAttribute parentAttribute)
            : base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
        {
            AddSymbolName(name, nameSpan);
        }

		public NameExpression NameExpression
		{
			get
			{
				return GetChildExpression<NameExpression>();
			}
		}

		public LiteralValueExpression Value
		{
			get
			{
				return GetChildExpression<LiteralValueExpression>();
			}
		}

		public bool HasReferencedXName
		{
			get
			{
				if (NameExpression != null)
				{
					return NameExpression.NameValue.HasValue;
				}

				if (Value != null)
				{
					return Value.HasValue;
				}

				return false;
			}
		}

        public TextSpan ReferencedXNameSpan
		{
			get
			{
				if (NameExpression != null)
				{
					return NameExpression.NameValue.Span;
				}

				if (Value != null)
				{
					return Value.Span;
				}

                return default(TextSpan);
			}
		}

		public string ReferencedXNameValue
		{
			get
			{
				if (NameExpression != null)
				{
					return NameExpression.NameValue.Value;
				}

				if (Value != null)
				{
					return Value.Value;
				}

				return "";
			}
		}

		public override string ToString()
		{
			string content = this.MarkupExtension + " ";

			if (NameExpression != null && NameExpression.NameValue != null)
			{
				content += ReferencedXNameValue;
			}

			return content;
		}
	}
}

