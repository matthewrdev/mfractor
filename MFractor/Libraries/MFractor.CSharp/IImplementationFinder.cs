using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Services
{
    /// <summary>
    /// The Implementation Finder will find all symbols for a given syntax node within a project or solution.
    /// </summary>
    public interface IImplementationFinder
    {
        /// <summary>
        /// For a given syntax node, find the type that it resolves to.
        /// </summary>
        /// <returns>The type.</returns>
        /// <param name="model">Model.</param>
        /// <param name="syntax">Syntax.</param>
        INamedTypeSymbol GetType(SemanticModel model, SyntaxNode syntax);

        /// <summary>
        /// For the given syntax node, find the method and type that it resolves to.
        /// </summary>
        /// <returns>The member symbol.</returns>
        /// <param name="model">Model.</param>
        /// <param name="syntax">Syntax.</param>
        /// <param name="type">Type.</param>
        ISymbol GetMemberSymbol(SemanticModel model, SyntaxNode syntax, out INamedTypeSymbol type);

        /// <summary>
        /// Finds the implementations for the given syntax node.
        /// </summary>
        /// <returns>The implementations.</returns>
        /// <param name="project">Project.</param>
        /// <param name="compilation">Compilation.</param>
        /// <param name="model">Model.</param>
        /// <param name="syntax">Syntax.</param>
        IEnumerable<ISymbol> FindImplementations(Project project, Compilation compilation, SemanticModel model, SyntaxNode syntax);
    }
}
