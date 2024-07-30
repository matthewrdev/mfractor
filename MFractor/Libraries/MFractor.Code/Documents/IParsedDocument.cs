using System;
using MFractor.Workspace;

namespace MFractor.Code.Documents
{
    /// <summary>
    /// A parsed
    /// </summary>
    public interface IParsedDocument
    {
        /// <summary>
        /// The project file that this parsed file is for.
        /// </summary>
        /// <value>The project file.</value>
        IProjectFile ProjectFile { get; }

        /// <summary>
        /// The full file path of the parsed document
        /// </summary>
        /// <value>The file path.</value>
        string FilePath { get; }

        /// <summary>
        /// The extension of this document.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// The name of the parsed document.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// The abstract syntax tree for this parsed document.
        /// </summary>
        object AbstractSyntaxTree { get; }
    }
}
