using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    public class DefaultMarkupExtensionExpression : MarkupExtensionExpression
    {
        public DefaultMarkupExtensionExpression(string name,
                                                TextSpan nameSpan,
                                                TextSpan expressionSpan,
                                                Expression parentExpression,
                                                XmlAttribute parentAttribute)
            : base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
        {
        }

        public List<ExpressionComponent> Components { get; set; }

        public override string ToString()
        {
            var content = "";

            content = string.Join(",", Components.Select(c => c.Content));

            return content;
        }
    }
}

