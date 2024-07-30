using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace.Data.Synchronisation
{
    /// <summary>
    /// A resource synchroniser that consumes non-text files.
    /// </summary>
    [InheritedExport]
    public interface IAssetResourceSynchroniser
    {
        /// <summary>
        /// The file extensions that this synchroniser supports.
        /// <para/>
        /// Each extension should include the '.'. 
        /// <para/>
        /// For example: { ".resx", ".xml", ".axml" }
        /// </summary>
        /// <value>The supported file extensions.</value>
        string[] SupportedFileExtensions { get; }

        /// <summary>
        /// Is this synchroniser available for the given <paramref name="solution"/> and <paramref name="project"/>.
        /// </summary>
        /// <returns><c>true</c>, if available was ised, <c>false</c> otherwise.</returns>
        /// <param name="solution">Solution.</param>
        /// <param name="project">Project.</param>
        bool IsAvailable(Solution solution, Project project);

        /// <summary>
        /// Decides if the resource synchroniser can perform a synchronisation pass for the provided project file.
        /// <para/>
        /// Typically, this method should decide if a synchronisation pass makes sense for the file type in the current context.
        /// <para/>
        /// For example, if this synchroniser consumes Android .xml resoures but the project is an iOS project then the synchroniser should return false.
        /// </summary>
        /// <returns><c>true</c>, if synchronise was caned, <c>false</c> otherwise.</returns>
        /// <param name="solution">Solution.</param>
        /// <param name="project">Project.</param>
        /// <param name="projectFile">The project file to synchronise.</param>
        Task<bool> CanSynchronise(Solution solution,
                            Project project,
                            IProjectFile projectFile);

        /// <summary>
        /// Synchronise the <paramref name="projectFile"/> into the provided <paramref name="database"/>.
        /// </summary>
        /// <returns>True if syncrhonisation was successful, false if not.</returns>
        /// <param name="solution">Solution.</param>
        /// <param name="project">Project.</param>
        /// <param name="projectFile">The project file to synchronise.</param>
        /// <param name="database">Database.</param>
        Task<bool> Synchronise(Solution solution,
                               Project project,
                               IProjectFile projectFile,
                               IProjectResourcesDatabase database);
    }
}
