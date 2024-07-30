using System;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class StaticBindingExpression : MarkupExtensionExpression
	{
		public StaticBindingExpression(string name,
									  TextSpan nameSpan,
									  TextSpan expressionSpan,
									  Expression parentExpression,
										 XmlAttribute parentAttribute)
			: base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
		{
		}

        public bool HasValue => GetChildExpression<LiteralValueExpression>() != null;

        public DotNetTypeSymbolExpression SymbolExpression => GetChildExpression<DotNetTypeSymbolExpression>((arg) => !arg.IsMarkupExtension);

        public LiteralValueExpression ValueExpression => GetChildExpression<LiteralValueExpression>();

        public override string ToString()
		{
			var content = MarkupExtension + " ";

			if (SymbolExpression != null)
			{
				content += SymbolExpression.ToString();
			}
			else if (ValueExpression != null)
			{
				content += ValueExpression.ToString();
			}

			return content;
		}
	}
}

