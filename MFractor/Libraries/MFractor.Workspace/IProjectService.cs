using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace
{
    /// <summary>
    /// The project information service can be used to inspect a <see cref="Microsoft.CodeAnalysis.Project"/> for a variety of information not exposed by Roslyn.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Checks if the <paramref name="projectIdentifier"/> points to a Shared Assets project type in the outer IDE.
        /// </summary>
        /// <returns><c>true</c>, if shared assets project was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectIdentifier">Project identifier.</param>
        bool IsSharedAssetsProject(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Get the default namespace for the provided project.
        /// </summary>
        /// <returns>The default namespace.</returns>
        /// <param name="project">Project.</param>
        string GetDefaultNamespace(Project project);

        /// <summary>
        /// Gets the default namespace.
        /// </summary>
        /// <returns>The default namespace.</returns>
        /// <param name="projectIdentifier">Project.</param>
        string GetDefaultNamespace(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Get the default namespace for the given <paramref name="filePath"/> within the <paramref name="project"/> based on its directory path.
        /// </summary>
        /// <returns>The default namespace.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">File path.</param>
        string GetDefaultNamespace(Project project, string filePath);

        /// <summary>
        /// Get the GUID for the provided <paramref name="project"/>.
        /// </summary>
        /// <returns>The project GUID.</returns>
        /// <param name="project">Project.</param>
        string GetProjectGuid(Project project);

        /// <summary>
        /// Gets the Roslyn project object that has the provided <paramref name="guid"/>.
        /// </summary>
        /// <returns>The project.</returns>
        /// <param name="guid">GUID.</param>
        Project GetProject(string guid);

        /// <summary>
        /// Gets the Roslyn project object that matches the provided <paramref name="projectIdentifier"/>
        /// </summary>
        /// <returns>The project.</returns>
        /// <param name="projectIdentifier">Project identifier.</param>
        Project GetProject(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Gets all the project files for the provided project.
        /// <para/>
        /// This method includes all files in a project, including non-code files.
        /// </summary>
        /// <returns>The project files.</returns>
        /// <param name="project">Project.</param>
        IReadOnlyList<IProjectFile> GetProjectFiles(Project project);

        /// <summary>
        /// Gets all the project files for the provided project.
        /// <para/>
        /// This method includes all files in a project, including non-code files.
        /// </summary>
        /// <returns>The project files.</returns>
        /// <param name="projectIdentifier">Project.</param>
        IReadOnlyList<IProjectFile> GetProjectFiles(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Gets the compilation for the given <paramref name="projectIdentifier"/>, considering if <paramref name="resolveIfSharedAssetsProject"/> is true.
        /// <para/>
        /// When the given <paramref name="projectIdentifier"/> references a Shared Assets Project, some times resolving the compilation makes no sense as the compilation can change depending on the active configuration.
        /// <para/>
        /// By default, the <see cref="GetCompilation(ProjectIdentifier, bool)"/> method will resolve the compilation regardless.
        /// </summary>
        /// <returns>The compilation.</returns>
        /// <param name="projectIdentifier">Project identifier.</param>
        /// <param name="resolveIfSharedAssetsProject">If the given <paramref name="projectIdentifier"/> is a shared assets <paramref name="projectIdentifier"/>, should it attempt to resolve the compilation?</param>
        Compilation GetCompilation(ProjectIdentifier projectIdentifier, bool resolveIfSharedAssetsProject = true);

        /// <summary>
        /// Gets the project file with the <paramref name="filePath"/> for the given <paramref name="project"/>.
        /// </summary>
        /// <returns>The project file.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">File path.</param>
        IProjectFile GetProjectFileWithFilePath(Project project, string filePath);

        /// <summary>
        /// Gets the project file with the <paramref name="filePath"/> for the given <paramref name="projectIdentifier"/>.
        /// </summary>
        /// <returns>The project file.</returns>
        /// <param name="projectIdentifier">Project.</param>
        /// <param name="filePath">File path.</param>
        IProjectFile GetProjectFileWithFilePath(ProjectIdentifier projectIdentifier, string filePath);

        /// <summary>
        /// Gets the files within the provided <paramref name="project"/> with the provided <paramref name="extension"/>.
        /// </summary>
        /// <returns>The project files with extension.</returns>
        /// <param name="project">Project.</param>
        /// <param name="extension">Extension.</param>
        /// <param name="stringComparison">The string comparison type</param>
        IReadOnlyList<IProjectFile> GetProjectFilesWithExtension(Project project, string extension, StringComparison stringComparison = StringComparison.Ordinal);

        /// <summary>
        /// Gets the files within the provided <paramref name="project"/> with the specified <paramref name="buildAction"/>.
        /// </summary>
        /// <returns>The project files with build action.</returns>
        /// <param name="project">Project.</param>
        /// <param name="buildAction">Build action.</param>
        /// <param name="stringComparison">String comparison.</param>
        IReadOnlyList<IProjectFile> GetProjectFilesWithBuildAction(Project project, string buildAction, StringComparison stringComparison = StringComparison.Ordinal);

        /// <summary>
        /// Finds the project file.
        /// </summary>
        /// <returns>The project file.</returns>
        /// <param name="project">Project.</param>
        /// <param name="predicate">The search function, accepting the full file path of the project file.</param>
        IProjectFile FindProjectFile(Project project, Func<string, bool> predicate);

        /// <summary>
        /// Finds the project file.
        /// </summary>
        /// <returns>The project file.</returns>
        /// <param name="projectIdentifier">projectIdentifier.</param>
        /// <param name="predicate">The search function, accepting the full file path of the project file.</param>
        IProjectFile FindProjectFile(ProjectIdentifier projectIdentifier, Func<string, bool> predicate);

        /// <summary>
        /// Delete the project file with the given <paramref name="filePath"/> from the <paramref name="project"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="project"></param>
        void DeleteProjectFile(string filePath, Project project, bool confirmDeletion = true);

        void DeleteProjectFile(IProjectFile projectFile, bool confirmDeletion = true);

        void DeleteProjectFiles(IReadOnlyList<string> filePaths, Project project, bool confirmDeletion = true);

        void DeleteProjectFiles(IReadOnlyList<IProjectFile> projectFiles, bool confirmDeletion = true);

        string GetProjectPath(ProjectIdentifier projectIdentifier);
        IReadOnlyList<IProjectFile> GetProjectFiles(Project project, IReadOnlyList<string> projectFolders);
    }
}
