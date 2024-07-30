using System;
using System.Linq;
using MFractor.Maui.Syntax.Parsers;

namespace MFractor.Maui.Syntax
{
    public static class ExpressionFactory
    {
        public static XamlExpressionSyntaxNode CreateExpression(ExpressionParserState state)
        {
            if (state == null)
            {
                return null;
            }

            XamlExpressionSyntaxNode syntax = null;
            switch (state.Kind)
            {
                case XamlExpressionSyntaxKind.Expression:
                    {
                        var expression = new ExpressionSyntax();
                        syntax = expression;
                    }
                    break;
                case XamlExpressionSyntaxKind.Symbol:
                    {
                        if (state.IsLeaf)
                        {
                            syntax = new TypeNameSyntax()
                            {
                                Name = state.Content,
                            };
                        }
                        else
                        {
                            syntax = new SymbolSyntax();
                        }
                    }
                    break;
                case XamlExpressionSyntaxKind.Namespace:
                    {
                        syntax = new NamespaceSyntax()
                        {
                            Namespace = state.Content,
                        };
                    }
                    break;
                case XamlExpressionSyntaxKind.TypeName:
                    {
                        syntax = new TypeNameSyntax()
                        {
                            Name = state.Content,
                        };
                    }
                    break;
                case XamlExpressionSyntaxKind.MemberAccessExpression:
                    {
                        var expression = new MemberAccessExpressionSyntax();

                        syntax = expression;
                    }
                    break;
                case XamlExpressionSyntaxKind.MemberName:
                    {
                        syntax = new MemberNameSyntax()
                        {
                            MemberName = state.Content,
                        };
                    }
                    break;
                case XamlExpressionSyntaxKind.Content:
                    {
                        syntax = new ContentSyntax()
                        {
                            Content = state.Content,
                        };
                    }
                    break;
                case XamlExpressionSyntaxKind.Assignment:
                    {
                        var assignment = new AssignmentSyntax();

                        syntax = assignment;
                    }
                    break;
                case XamlExpressionSyntaxKind.Property:
                    {
                        syntax = new PropertySyntax()
                        {
                            PropertyName = state.Content,
                        };
                    }
                    break;
                case XamlExpressionSyntaxKind.Value:
                    {
                        syntax = new ValueSyntax()
                        {
                            Value = state.Content,
                        };
                    }
                    break;
                case XamlExpressionSyntaxKind.StringValue:
                    {
                        syntax = new StringValueSyntax()
                        {
                            Value = state.Content,
                        };
                    }
                    break;
                case XamlExpressionSyntaxKind.Error:
                    {
                        syntax = new ErrorSyntax()
                        {
                            Content = state.Content,
                        };
                    }
                    break;
            }

            if (state.Children.Any())
            {
                foreach (var ch in state.Children)
                {
                    var childSyntax = CreateExpression(ch);

                    if (childSyntax != null)
                    {
                        syntax?.AddChild(childSyntax);
                    }

                }
            }

            if (syntax != null)
            {
                syntax.Span = state.Span;
                syntax.FullSpan = state.FullSpan;
                syntax.SetLeading(state.Leading);
                syntax.SetTrailing(state.Trailing);
            }

            return syntax;
        }
    }
}
