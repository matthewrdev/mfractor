using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Utilities.SyntaxWalkers
{
    public class StringSyntaxWalker : SyntaxWalker
    {
        public readonly List<LiteralExpressionSyntax> LiteralExpressionSyntax = new List<LiteralExpressionSyntax>();

        public readonly List<InterpolatedStringTextSyntax> InterpolatedStrings = new List<InterpolatedStringTextSyntax>();

        public override void Visit(SyntaxNode node)
        {
            var literal = ResolveLiteralExpressionSyntax(node);

            if (literal == null)
            {
                base.Visit(node);
            }
            else
            {
                var kind = literal.Token.Kind();

                if (kind == SyntaxKind.StringLiteralToken)
                {
                    LiteralExpressionSyntax.Add(literal);
                }
            }
        }

        public static LiteralExpressionSyntax ResolveLiteralExpressionSyntax(SyntaxNode syntax)
        {
            var literalExpression = syntax as LiteralExpressionSyntax;
            if (literalExpression == null)
            {
                var argSyntax = syntax as ArgumentSyntax;

                if (argSyntax != null)
                {
                    return argSyntax.Expression as LiteralExpressionSyntax;
                }

                return null;
            }

            if (IsMethodParameterDefault(literalExpression))
            {
                return null;
            }

            if (IsConstExpression(literalExpression))
            {
                return null;
            }

            return literalExpression;
        }

        public static bool IsMethodParameterDefault(LiteralExpressionSyntax literalExpression)
        {
            if (literalExpression.Parent is EqualsValueClauseSyntax
                && literalExpression.Parent.Parent is ParameterSyntax
                && literalExpression.Parent.Parent.Parent is ParameterListSyntax)
            {
                return true;
            }

            return false;
        }

        public static bool IsConstExpression(LiteralExpressionSyntax literalExpression)
        {
            if (literalExpression.Parent is EqualsValueClauseSyntax
                && literalExpression.Parent.Parent is VariableDeclaratorSyntax
                && literalExpression.Parent.Parent.Parent is VariableDeclarationSyntax
                && literalExpression.Parent.Parent.Parent.Parent is FieldDeclarationSyntax)
            {
                var field = literalExpression.Parent.Parent.Parent.Parent as FieldDeclarationSyntax;

                if (field.Modifiers.Any(SyntaxKind.ConstKeyword))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
