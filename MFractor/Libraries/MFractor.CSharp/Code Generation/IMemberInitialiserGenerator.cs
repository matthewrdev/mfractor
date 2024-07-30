using System;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IMemberInitialiserGenerator : ICodeGenerator
    {
        bool ForceStringLiteral { get; set; }

        EqualsValueClauseSyntax GenerateSyntax(ITypeSymbol initialiserType, string initialiserValue);
    }
}
