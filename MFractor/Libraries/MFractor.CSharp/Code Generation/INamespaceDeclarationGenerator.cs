using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    /// <summary>
    /// The <see cref="INamespaceDeclarationGenerator"/> creates <see cref="NamespaceDeclarationSyntax"/> nodes 
    /// </summary>
    public interface INamespaceDeclarationGenerator : ICodeGenerator
    {
        /// <summary>
        /// Given the <paramref name="namespace"/>, removes all invalid characters and converts it into a compliant C# symbol name.
        /// </summary>
        /// <returns>The namespace name.</returns>
        /// <param name="namespace">Namespace.</param>
        string CleanNamespaceName(string @namespace);

        /// <summary>
        /// Creates a default namespace for the <paramref name="folderPath"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The namespace for.</returns>
        /// <param name="project">Project.</param>
        /// <param name="folderPath">Folder path.</param>
        string GetNamespaceFor(Project project, IEnumerable<string> folderPath);

        /// <summary>
        /// Creates a default namespace for the <paramref name="folderPath"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The namespace for.</returns>
        /// <param name="projectIdentifier">Project.</param>
        /// <param name="folderPath">Folder path.</param>
        string GetNamespaceFor(ProjectIdentifier projectIdentifier, IEnumerable<string> folderPath);

        /// <summary>
        /// Creates a default namespace for the <paramref name="folderPath"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The namespace for.</returns>
        /// <param name="project">Project.</param>
        /// <param name="folderPath">Folder path.</param>
        string GetNamespaceFor(Project project, string folderPath);

        /// <summary>
        /// Creates a default namespace for the <paramref name="folderPath"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The namespace for.</returns>
        /// <param name="projectIdentifier">Project.</param>
        /// <param name="folderPath">Folder path.</param>
        string GetNamespaceFor(ProjectIdentifier projectIdentifier, string folderPath);

        /// <summary>
        /// Creates a default namespace for the <paramref name="projectFile"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The namespace for.</returns>
        /// <param name="project">Project.</param>
        /// <param name="projectFile">Project file.</param>
        string GetNamespaceFor(Project project, IProjectFile projectFile);

        /// <summary>
        /// Creates a default namespace for the <paramref name="projectFile"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The namespace for.</returns>
        /// <param name="projectIdentifier">Project.</param>
        /// <param name="projectFile">Project file.</param>
        string GetNamespaceFor(ProjectIdentifier projectIdentifier, IProjectFile projectFile);

        NamespaceDeclarationSyntax GenerateSyntax(string @namespace);
    }
}
