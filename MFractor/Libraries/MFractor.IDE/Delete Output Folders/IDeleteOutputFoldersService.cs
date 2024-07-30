using System;
using System.Collections.Generic;
using MFractor.Progress;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.DeleteOutputFolders
{
    /// <summary>
    /// The <see cref="IDeleteOutputFoldersService"/> removes the output and working folders for a given project or solution.
    /// </summary>
    public interface IDeleteOutputFoldersService
    {
        /// <summary>
        /// Delete the ouput folders for the given <paramref name="solution"/> using the provided <paramref name="options"/>.
        /// <para/>
        /// When deleting the output folders for all child projects of the <paramref name="solution"/>, the <paramref name="options"/> will be used rather than the project specific settings.
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="options"></param>
        /// <param name="progressMonitor"></param>
        void DeleteOutputFolders(Solution solution, IDeleteOutputFoldersOptions options, bool deleteProjectArtifacts, IProgressMonitor progressMonitor);

        /// <summary>
        /// Delete the output folders for the given <paramref name="projects"/> using the <paramref name="options"/>.
        /// <para/>
        /// When deleting the output folders, the <paramref name="options"/> will be used instead of the project specific options.
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="options"></param>
        /// <param name="progressMonitor"></param>
        void DeleteOutputFolders(IEnumerable<Project> projects, IDeleteOutputFoldersOptions options, IProgressMonitor progressMonitor);

        /// <summary>
        /// Delete the output folders of the given <paramref name="projects"/> using the options associated to each project.
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="progressMonitor"></param>
        void DeleteOutputFolders(IReadOnlyDictionary<Project, IDeleteOutputFoldersOptions> projects, IProgressMonitor progressMonitor);

        /// <summary>
        /// Delete the output folders for the given <paramref name="project"/> using the provided <paramref name="options"/>.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="options"></param>
        /// <param name="progressMonitor"></param>
        void DeleteOutputFolders(Project project, IDeleteOutputFoldersOptions options, IProgressMonitor progressMonitor);
    }
}
