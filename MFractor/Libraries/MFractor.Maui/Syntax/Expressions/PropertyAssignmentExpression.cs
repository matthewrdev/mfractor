using System;

using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    public abstract class PropertyAssignmentExpression : PropertyExpression
    {
        public PropertyAssignmentExpression(string name,
                                                    TextSpan nameSpan,
                                                    TextSpan expressionSpan,
                                                    Expression parentExpression,
                                                    XmlAttribute parentAttribute)
            : base(name, nameSpan, expressionSpan, parentExpression, parentAttribute)
        {
        }

        public Expression AssignmentValue
        {
            get
            {
                return GetChildExpression<Expression>((arg) =>
               {
                   if (arg is DotNetTypeSymbolExpression dotNetTypeSymbolExpression
                        && dotNetTypeSymbolExpression.IsMarkupExtension)
                   {
                       return false;
                   }

                   return true;
               });
            }
        }
    }
}

