using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace
{
    /// <summary>
    /// A file that is included within a project. 
    /// <para/>
    /// This could be a code file (EG: C#, F# etc), an asset (EG: Image, XAML) or a configuration file.
    /// </summary>
    public interface IProjectFile
    {
        /// <summary>
        /// The project that this project file belongs to.
        /// </summary>
        Project CompilationProject { get; }

        /// <summary>
        /// The <see cref="Document"/> in the <see cref="CompilationProject"/> for this <see cref="IProjectFile"/>.
        /// <para/>
        /// May be null if this project file is an asset file and not a compilable document.
        /// </summary>
        Document CompilationDocument { get; }

        /// <summary>
        /// The full file path for where this project file exists on disk.
        /// </summary>
        /// <value>The file path.</value>
        string FilePath { get; }

        /// <summary>
        /// Gets the virtual path, aka, the path of this file relative to the root of the project.
        /// </summary>
        string VirtualPath { get; }

        /// <summary>
        /// The file extension of the project file (including the leading ".").
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Gets the file info.
        /// </summary>
        FileInfo FileInfo { get; }

        /// <summary>
        /// The name of this project file, excluding the file path component.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The folders that this file is nested within.
        /// </summary>
        IReadOnlyList<string> ProjectFolders { get; }

        /// <summary>
        /// The build action, if any, for this project file.
        /// </summary>
        string BuildAction { get; }

        /// <summary>
        /// Is this project file a linked file rather than physical file on disk?
        /// </summary>
        bool IsLink { get; }

        /// <summary>
        /// If this project file is an EmbeddedResource, what is it's resource id?
        /// </summary>
        string ResourceId { get; }
    }
}
