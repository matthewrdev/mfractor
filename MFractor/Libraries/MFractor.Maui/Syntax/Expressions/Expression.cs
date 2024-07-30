using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    public abstract class Expression
    {
        public Expression(TextSpan expressionSpan,
                          Expression parent,
                          XmlAttribute parentAttribute)
        {
            ParentExpression = parent;
            ParentAttribute = parentAttribute;
            Span = expressionSpan;
        }

        public Expression ParentExpression { get; set; }

        public bool IsRootExpression => ParentExpression == null;

        public bool IsLeafExpression => !HasChildren;

        protected void AddSymbolName(string name, 
                                     TextSpan nameRegion)
        {
            var nameExpression = new DotNetTypeSymbolExpression(name, nameRegion, this, this.ParentAttribute, true);
            Children.Add(nameExpression);
        }

        public XmlAttribute ParentAttribute { get; set; }

        public bool IsMalformed { get; set; }

        public TextSpan Span { get; set; }

        public List<Expression> Children { get; private set; } = new List<Expression>();

        public bool HasChildren => Children != null && Children.Count > 0;

        public TExpression GetChildExpression<TExpression>() where TExpression : Expression
        {
            if (!HasChildren)
            {
                return null;
            }

            var candidates = Children.OfType<TExpression>();

            return candidates.FirstOrDefault();
        }

        public TXamlExpression GetChildExpression<TXamlExpression>(Func<TXamlExpression, bool> matchFunc) where TXamlExpression : Expression
        {
            if (!HasChildren)
            {
                return null;
            }

            var candidates = Children.OfType<TXamlExpression>();

            return candidates.FirstOrDefault(matchFunc);
        }

        public bool HasChildExpression<TXamlExpression>() where TXamlExpression : Expression
        {
            if (!HasChildren)
            {
                return false;
            }

            var candidates = Children.OfType<TXamlExpression>();

            return candidates.Any();
        }

        public Expression ExpressionAtPosition(int position)
        {
            if (!FileLocationHelper.IsBetween(position, Span))
            {
                return null;
            }

            var expression = this;

            if (HasChildren)
            {
                foreach (var child in Children)
                {
                    var temp = child.ExpressionAtPosition(position);
                    if (temp != null)
                    {
                        expression = temp;
                        break;
                    }
                }
            }

            return expression;
        }
    }
}

