using System;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    public class DynamicResourceExpression : MarkupExtensionExpression
    {
        public DynamicResourceExpression(string name,
                                      TextSpan nameSpan,
                                      TextSpan expressionSpan,
                                      Expression parentExpression,
                                         XmlAttribute parentAttribute)
            : base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
        {
        }

        public LiteralValueExpression Value => GetChildExpression<LiteralValueExpression>();
    }
}
