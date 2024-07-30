using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
	public abstract class PropertyExpression : Expression
	{
		public PropertyExpression(string name, 
                                  TextSpan nameSpan,
                                  TextSpan expressionSpan,
		                                  Expression parentExpression,
		                                  XmlAttribute parentAttribute)
			: base(expressionSpan, parentExpression, parentAttribute)
		{
			AddSymbolName(name, nameSpan);
		}

        public DotNetTypeSymbolExpression PropertyName => GetChildExpression<DotNetTypeSymbolExpression>();
    }
}

