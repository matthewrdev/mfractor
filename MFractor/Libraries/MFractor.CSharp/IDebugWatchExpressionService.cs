using System;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Services
{
    public interface IDebugWatchExpressionService
    {
        string GetDebugWatchExpression(Compilation compilation, SyntaxNode syntaxNode);
    }
}
