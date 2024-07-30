using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Services
{
    public interface ITypeSyntaxSimplifier
    {
        IReadOnlyList<UsingDirectiveSyntax> GetDeduplicatedUsings(SyntaxTree syntaxTree, IReadOnlyList<UsingDirectiveSyntax> usings);
        IReadOnlyList<UsingDirectiveSyntax> GetReducedTypeUsings(TypeSyntax typeSyntax, Compilation compilation, out SyntaxNode reducedSyntaxNode, out QualifiedNameSyntax qualifiedNameSyntax);
    }
}