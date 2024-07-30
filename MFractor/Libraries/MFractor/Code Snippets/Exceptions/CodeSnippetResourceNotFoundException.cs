using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace MFractor.CodeSnippets.Exceptions
{
    /// <summary>
    /// The code snippet resource was not found.
    /// </summary>
    public class CodeSnippetResourceNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Exceptions.CodeSnippetResourceNotFoundException"/> class.
        /// </summary>
        /// <param name="snippetName">The name of the code snippet.</param>
        /// <param name="assembly">Assembly.</param>
        public CodeSnippetResourceNotFoundException(string snippetName, Assembly assembly) 
            : base($"Unable to locate a code snippet resource that matches {snippetName} in the assembly {assembly.GetName().Name}")
        {
        }
    }
}
