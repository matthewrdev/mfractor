using System;
using System.Collections.Generic;
using MFractor.Ide;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Code
{
    /// <summary>
    /// The <see cref="ICSharpSyntaxReducer"/> inspects <see cref="SyntaxNode"/>'s and converts any full or partial type qualifications into using directives and reduces the original type usages to their terser formats.
    /// <para/>
    /// For example, the qualified type 'System.IO.FileInfo' would be reduced into a using statement, "using System.IO;", and a simplified type qualification, "FileInfo".
    /// </summary>
    public interface ICSharpSyntaxReducer : IFileCreationPostProcessor
    {
        /// <summary>
        /// Reduce the specified syntax, model, knownTypes and usings.
        /// </summary>
        /// <returns>The reduced <paramref name="syntax"/>.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="model">Model.</param>
        /// <param name="knownTypes">Known types.</param>
        /// <param name="usings">Usings.</param>
        SyntaxNode Reduce(SyntaxNode syntax, SemanticModel model, List<string> knownTypes, ref List<UsingDirectiveSyntax> usings);

        /// <summary>
        /// Deduplicates and sorts the provided using directives.
        /// </summary>
        /// <returns>The and sort usings.</returns>
        /// <param name="existing">Existing.</param>
        /// <param name="additional">Additional.</param>
        IReadOnlyList<UsingDirectiveSyntax> DeduplicateAndSortUsings(IReadOnlyList<UsingDirectiveSyntax> existing, IReadOnlyList<UsingDirectiveSyntax> additional);
        string Reduce(string code, Project project);
    }
}
