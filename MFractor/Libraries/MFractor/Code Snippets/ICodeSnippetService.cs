using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.CodeSnippets
{
    /// <summary>
    /// The code snippet service provides code snippets that are available both from the outer IDE.
    /// </summary>
    public interface ICodeSnippetService
    {
        /// <summary>
        /// Gets the code snippet by identifier.
        /// </summary>
        /// <returns>The code snippet by identifier.</returns>
        /// <param name="id">Identifier.</param>
        ICodeSnippet GetCodeSnippetById(string id);

        /// <summary>
        /// All available code snippets from the outer IDE.
        /// </summary>
        /// <value>The snippets.</value>
        IEnumerable<ICodeSnippet> Snippets { get; }
    }
}
