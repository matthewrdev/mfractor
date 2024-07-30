using System;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    public class StaticResourceExpression : MarkupExtensionExpression
    {
        public StaticResourceExpression(string name,
                                      TextSpan nameSpan,
                                      TextSpan expressionSpan,
                                      Expression parentExpression,
                                         XmlAttribute parentAttribute)
            : base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
        {
            AddSymbolName(name, nameSpan);
        }

        public LiteralValueExpression Value => GetChildExpression<LiteralValueExpression>();
    }
}

