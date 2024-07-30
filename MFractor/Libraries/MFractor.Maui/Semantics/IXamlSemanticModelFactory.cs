using System;
using MFractor.Maui.Xmlns;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Semantics
{
    /// <summary>
    /// A factory class for creating  new <see cref="IXamlSemanticModel"/> instances.
    /// </summary>
    public interface IXamlSemanticModelFactory
    {
        /// <summary>
        /// Create a new <see cref="IXamlSemanticModel"/> for the provided 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="project"></param>
        /// <param name="compilation"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        IXamlSemanticModel Create(IParsedXamlDocument document, Project project, Compilation compilation, IXamlNamespaceCollection namespaces);

        IXamlSemanticModel Create(IParsedXamlDocument document, Project project);
    }
}
