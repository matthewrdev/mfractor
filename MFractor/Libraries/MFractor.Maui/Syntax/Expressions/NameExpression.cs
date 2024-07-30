using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    public class NameExpression : PropertyAssignmentExpression
    {
        public NameExpression(string name,
                               TextSpan nameSpan,
                               TextSpan expressionSpan,
                               Expression parentExpression,
                               XmlAttribute parentAttribute)
            : base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
        {
        }

        public LiteralValueExpression NameValue
        {
            get
            {
                return GetChildExpression<LiteralValueExpression>();
            }
        }

        public override string ToString()
        {
            string content = PropertyName + "=";

            if (NameValue != null && NameValue.HasValue)
            {
                content += NameValue.Value;
            }

            return content;
        }
    }
}
