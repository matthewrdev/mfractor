using Microsoft.CodeAnalysis;

namespace MFractor.Workspace
{
    /// <summary>
    /// A folder that is included within a project. 
    /// </summary>
    public interface IProjectFolder
    {
        /// <summary>
        /// The project that this folder belongs to.
        /// </summary>
        /// <value>The project.</value>
        Project Project { get; }

        /// <summary>
        /// The full path for where this folder exists on disk.
        /// </summary>
        /// <value>The file path.</value>
        string Path { get; }

        /// <summary>
        /// Gets the virtual path of this project folder
        /// </summary>
        /// <value>The virtual path.</value>
        string VirtualPath { get; }

        /// <summary>
        /// The name of this folder, excluding the full path component.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}
