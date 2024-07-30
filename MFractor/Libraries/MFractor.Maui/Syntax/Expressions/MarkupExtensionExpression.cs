using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public class MarkupExtensionExpression : Expression
	{
		public MarkupExtensionExpression(string name, 
                                         TextSpan nameSpan, 
                                         TextSpan expressionSpan, 
		                                  Expression parent,
										 XmlAttribute parentAttribute)
			: base(expressionSpan, parent, parentAttribute)
		{
			AddSymbolName(name, nameSpan);
		}

        public DotNetTypeSymbolExpression MarkupExtension => GetChildExpression<DotNetTypeSymbolExpression>(arg => arg.IsMarkupExtension);
    }
}
