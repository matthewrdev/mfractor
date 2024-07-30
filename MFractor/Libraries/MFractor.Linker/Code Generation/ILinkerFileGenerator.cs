using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Linker.CodeGeneration
{
    /// <summary>
    /// The linker file generator creates a new linker file for a project or adds symbols to a project for linking.
    /// </summary>
    public interface ILinkerFileGenerator : ICodeGenerator
    {
        /// <summary>
        /// What is the default name of the linker file for a project?
        /// <para/>
        /// If a linker file already exists within a project, this file name may be different to what is actually within the project.
        /// </summary>
        /// <value>The default name of the linker file.</value>
        string DefaultLinkerFileName { get; set; }

        /// <summary>
        /// What is the default folder that the linker file should be placed inside a project?
        /// <para/>
        /// If a linker file already exists within a project, this folder path name may be different to what is actually within the project.
        /// </summary>
        /// <value>The default name of the linker file.</value>
        string DefaultLinkerFileFolder { get; set; }

        /// <summary>
        /// Gets the default linker file path, the <see cref="DefaultLinkerFileFolder"/> and <see cref="DefaultLinkerFileName"/>.
        /// </summary>
        /// <value>The default linker file path.</value>
        string DefaultLinkerFilePath { get; }

        /// <summary>
        /// Finds the linker file for a project.
        /// <para/>
        /// If no linker file exists within the <paramref name="project"/>, returns null.
        /// </summary>
        /// <returns>The linker file for project.</returns>
        /// <param name="project">Project.</param>
        IProjectFile GetLinkerFileForProject(Project project);

        /// <summary>
        /// Adds the symbols to project for linker.
        /// <para/>
        /// If the linker file does not exist, creates a new linker file and inserts a 
        /// </summary>
        /// <returns>The symbols to project for linker.</returns>
        /// <param name="project">Project.</param>
        /// <param name="symbols">Symbols.</param>
        IReadOnlyList<IWorkUnit> AddLinkedSymbols(Project project, IEnumerable<ISymbol> symbols);

        /// <summary>
        /// Creates a new linker file.
        /// </summary>
        /// <returns>The linker file.</returns>
        /// <param name="project">Project.</param>
        /// <param name="symbols">Symbols.</param>
        IReadOnlyList<IWorkUnit> CreateLinkerFile(Project project, IEnumerable<ISymbol> symbols);

        /// <summary>
        /// Creates a new linker file.
        /// </summary>
        /// <returns>The linker file.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">The file path to create the new linker configuration file at.</param>
        /// <param name="symbols">Symbols.</param>
        IReadOnlyList<IWorkUnit> CreateLinkerFile(Project project, string filePath, IEnumerable<ISymbol> symbols);

        /// <summary>
        /// Creates the content of the linker file.
        /// </summary>
        /// <returns>The linker file content.</returns>
        /// <param name="symbols">Symbols.</param>
        /// <param name="formattingPolicy">Formatting policy.</param>
        string CreateLinkerFileContent(IEnumerable<ISymbol> symbols, IXmlFormattingPolicy formattingPolicy);
    }
}
