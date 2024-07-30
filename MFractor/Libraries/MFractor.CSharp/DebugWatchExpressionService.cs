using System;
using System.ComponentModel.Composition;
using MFractor.CSharp.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDebugWatchExpressionService))]
    class DebugWatchExpressionService : IDebugWatchExpressionService
    {
        public string GetDebugWatchExpression(Compilation compilation,SyntaxNode syntaxNode)
        {
            if (compilation == null 
                || syntaxNode == null)
            {
                return string.Empty;
            }

            var semanticModel = compilation.GetSemanticModel(syntaxNode.SyntaxTree);
            if (semanticModel == null)
            {
                return string.Empty;
            }

            var watchExpression = string.Empty;

            if (syntaxNode is ParameterSyntax p)
            {
                watchExpression = p.Identifier.Text;
            }
            else if (syntaxNode is IdentifierNameSyntax id)
            {
                watchExpression = ConvertIdentifierNameSyntaxToWatchExpression(semanticModel, id);
            }
            else if (syntaxNode is VariableDeclaratorSyntax vd)
            {
                watchExpression = vd.Identifier.Text;
            }
            else if (syntaxNode is ArgumentSyntax a
                     && a?.Expression is IdentifierNameSyntax idn)
            {
                watchExpression = idn.Identifier.Text;
            }

            return watchExpression;
        }

        private string ConvertIdentifierNameSyntaxToWatchExpression(SemanticModel semanticModel, IdentifierNameSyntax id)
        {
            var watchExpression = string.Empty;

            if (id.Parent is MemberAccessExpressionSyntax memberAccess)
            {
                watchExpression = ConvertMemberAccessToWatchExpession(semanticModel, memberAccess);
            }
            else
            {
                var symbol = semanticModel.GetSymbolInfo(id);

                if (symbol.Symbol is IMethodSymbol)
                {
                    return string.Empty;
                }

                if (id.Parent is ParameterSyntax ps
                   && ps.Type == id)
                {
                    return string.Empty;
                }

                if (id.Parent is VariableDeclarationSyntax vds
                   && vds.Type == id)
                {
                    return string.Empty;
                }

                if (id.Parent is ObjectCreationExpressionSyntax)
                {
                    return string.Empty;
                }

                watchExpression = id.Identifier.Text;
            }

            return watchExpression;
        }

        string ConvertMemberAccessToWatchExpession(SemanticModel semanticModel, MemberAccessExpressionSyntax memberAccess)
        {
            var symbol = semanticModel.GetSymbolInfo(memberAccess);

            if (symbol.Symbol is IMethodSymbol)
            {
                return string.Empty;
            }

            var result = memberAccess;
            SyntaxNode parent = memberAccess;
            while (parent is MemberAccessExpressionSyntax)
            {
                parent = result.Parent;

                if (parent is MemberAccessExpressionSyntax m)
                {
                    result = m;
                }
                else if (parent is InvocationExpressionSyntax)
                {
                    return result.Expression.ToString();
                }
            }

            return result.ToString();
        }
    }
}