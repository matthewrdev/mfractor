using System;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Services
{
    public interface IColorEvaluationService
    {
        ColorEvalautionResult Evaluate(Project project, string filePath, int position);
        ColorEvalautionResult Evaluate(SyntaxNode syntaxNode);
    }
}