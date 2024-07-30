using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IExceptionHandlerGenerator : ICodeGenerator
    {
        ICodeSnippet Snippet { get; set; }

        StatementSyntax GenerateSyntax(string exceptionName, INamedTypeSymbol exceptionTypeSymbol);
        StatementSyntax GenerateSyntax(string exceptionName, string exceptionType);

        string GenerateText(string exceptionName, INamedTypeSymbol exceptionTypeSymbol);
        string GenerateText(string exceptionName, string exceptionType);
    }
}
