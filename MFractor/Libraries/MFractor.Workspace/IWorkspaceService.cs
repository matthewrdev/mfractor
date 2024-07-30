using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.IOC;
using Microsoft.CodeAnalysis;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Workspace
{
    /// <summary>
    /// The event arguments for when a Workspace is added or removed.
    /// </summary>
    public class WorkspaceAddedOrRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// The workspace that was added or removed.
        /// </summary>
        /// <value>The workspace.</value>
        public CompilationWorkspace Workspace { get; }

        /// <summary>
        /// Initializes a new <see cref="WorkspaceAddedOrRemovedEventArgs"/> instance.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        public WorkspaceAddedOrRemovedEventArgs(CompilationWorkspace workspace)
        {
            Workspace = workspace;
        }
    }

    /// <summary>
    /// The event arguments for when a Workspace is added or removed.
    /// </summary>
    public class WorkspaceExecutionTargetChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new workspace execution target that was added or removed.
        /// </summary>
        /// <value>The workspace.</value>
        public CompilationWorkspace Workspace { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.WorkspaceAddedOrRemovedEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        public WorkspaceExecutionTargetChangedEventArgs(CompilationWorkspace workspace)
        {
            Workspace = workspace;
        }
    }

    /// <summary>
    /// The event arguments for when a solution is opened.
    /// </summary>
    public class SolutionOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the solution that was opened.
        /// <para/>
        /// Use <see cref="IWorkspaceService.GetSolution(string)"/> to retrieve the solution.
        /// </summary>
        /// <value>The name of the solution.</value>
        public string SolutionName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.SolutionOpenedEventArgs"/> class.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        public SolutionOpenedEventArgs(string solutionName)
        {
            SolutionName = solutionName;
        }
    }

    /// <summary>
    /// The event arguments for when a solution is closed.
    /// </summary>
    public class SolutionClosedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the solution that was closed.
        /// </summary>
        /// <value>The name of the solution.</value>
        public string SolutionName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.SolutionClosedEventArgs"/> class.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        public SolutionClosedEventArgs(string solutionName)
        {
            SolutionName = solutionName;
        }
    }

    /// <summary>
    /// The event arguments for when a project is added to a solution.
    /// </summary>
    public class ProjectAddedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the solution that the project was added to.
        /// </summary>
        /// <value>The name of the solution.</value>
        public string SolutionName { get; }

        /// <summary>
        /// The GUID of the project that was added.
        /// </summary>
        /// <value>The project GUID.</value>
        public string ProjectGuid { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.ProjectAddedEventArgs"/> class.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        /// <param name="projectGuid">Project GUID.</param>
        public ProjectAddedEventArgs(string solutionName, string projectGuid)
        {
            SolutionName = solutionName;
            ProjectGuid = projectGuid;
        }
    }

    /// <summary>
    /// Occurs when a project is removed from a solution.
    /// </summary>
    public class ProjectRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the solution that the project was removed from.
        /// </summary>
        /// <value>The name of the solution.</value>
        public string SolutionName { get; }

        /// <summary>
        /// The GUID of the project that was removed.
        /// </summary>
        /// <value>The project GUID.</value>
        public string ProjectGuid { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.ProjectRemovedEventArgs"/> class.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        /// <param name="projectGuid">Project GUID.</param>
        public ProjectRemovedEventArgs(string solutionName, string projectGuid)
        {
            SolutionName = solutionName;
            ProjectGuid = projectGuid;
        }
    }

    /// <summary>
    /// Occurs when a reference such as an assembly or project is added to a project.
    /// </summary>
    public class ProjectReferenceAddedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the solution that contains the project modified.
        /// </summary>
        /// <value>The name of the solution.</value>
        public string SolutionName { get; }

        /// <summary>
        /// The GUID of the project that had a reference added.
        /// </summary>
        /// <value>The project GUID.</value>
        public string ProjectGuid { get; }

        /// <summary>
        /// Gets the name of the reference.
        /// </summary>
        /// <value>The name of the reference.</value>
        public string ReferenceName { get; }

        public ProjectReferenceAddedEventArgs(string solutionName, string projectGuid, string referenceName)
        {
            SolutionName = solutionName;
            ProjectGuid = projectGuid;
            ReferenceName = referenceName;
        }
    }

    /// <summary>
    /// Occurs when a reference is removed from a project.
    /// </summary>
    public class ProjectReferenceRemovedEventArgs : EventArgs
    {
        public string SolutionName { get; }

        public string ProjectGuid { get; }

        public string ReferenceName { get; }

        public ProjectReferenceRemovedEventArgs(string solutionName, string projectGuid, string referenceName)
        {
            SolutionName = solutionName;
            ProjectGuid = projectGuid;
            ReferenceName = referenceName;
        }
    }

    public class ProjectRenamedEventArgs : EventArgs
    {
        public string SolutionName { get; }

        public string ProjectGuid { get; }

        public string OldName { get; }

        public string NewName { get; }

        public ProjectRenamedEventArgs(string solutionName, string projectGuid, string oldName, string newName)
        {
            SolutionName = solutionName;
            ProjectGuid = projectGuid;
            OldName = oldName;
            NewName = newName;
        }
    }

    public class FilesEventArgs : EventArgs
    {
        public IReadOnlyList<string> ProjectGuids => ChangeSet.ProjectGuids;

        public IProjectFileChangeSet<string> ChangeSet { get; }

        public FilesEventArgs(IProjectFileChangeSet<string> changeSet)
        {
            ChangeSet = changeSet ?? new ProjectFileChangeSet<string>(); ;
        }

        public bool HasProject(Project project)
        {
            var guid = Resolver.Resolve<IProjectService>().GetProjectGuid(project);

            return HasProject(guid);
        }

        public bool HasProject(string projectGuid)
        {
            return ChangeSet.ContainsProject(projectGuid);
        }

        public IReadOnlyList<string> GetProjectFiles(string projectGuid)
        {
            return ChangeSet.GetChanges(projectGuid);
        }
    }

    public class RenamedFile
    {
        public RenamedFile(string oldFilePath, string newFilePath)
        {
            OldFilePath = oldFilePath;
            NewFilePath = newFilePath;
        }

        public string OldFilePath { get; }

        public string NewFilePath { get; }
    }

    /// <summary>
    /// The event arguments for when a file is renamed.
    /// </summary>
    public class FilesRenamedEventArgs : EventArgs
    {
        public IProjectFileChangeSet<RenamedFile> ChangeSet { get; }

        public FilesRenamedEventArgs(IProjectFileChangeSet<RenamedFile> changeSet)
        {
            ChangeSet = changeSet ?? new ProjectFileChangeSet<RenamedFile>();
        }

        public bool HasProject(Project project)
        {
            var guid = Resolver.Resolve<IProjectService>().GetProjectGuid(project);

            return HasProject(guid);
        }

        public bool HasProject(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return false;
            }

            return ChangeSet.ContainsProject(projectGuid);
        }

        public IReadOnlyList<RenamedFile> GetProjectFiles(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return new List<RenamedFile>();
            }

            return ChangeSet.GetChanges(projectGuid);
        }
    }

    /// <summary>
    /// The workspace service exposes the workspaces available in the current product and notifies of changes against those workspaces.
    /// </summary>
	public interface IWorkspaceService
    {
        /// <summary>
        /// Gets the currently active workspace.
        /// </summary>
        /// <value>The current workspace.</value>
		CompilationWorkspace CurrentWorkspace { get; }

        /// <summary>
        /// Gets all workspaces that are currently active within this product.
        /// </summary>
        /// <value>The workspaces.</value>
        IEnumerable<CompilationWorkspace> Workspaces { get; }

        /// <summary>
        /// Gets the solution named <paramref name="solutionName"/> or null if no solution with the name <paramref name="solutionName"/> exists.
        /// </summary>
        /// <returns>The solution.</returns>
        /// <param name="solutionName">Solution name.</param>
        Solution GetSolution(string solutionName);

        /// <summary>
        /// Occurs when on workspace is added.
        /// </summary>
        event EventHandler<WorkspaceAddedOrRemovedEventArgs> WorkspaceAdded;

        /// <summary>
        /// Occurs when on workspace is removed.
        /// </summary>
        event EventHandler<WorkspaceAddedOrRemovedEventArgs> WorkspaceRemoved;

        /// <summary>
        /// Occurs when the workspace execution target changes.
        /// <para/>
        /// For example, when a user changes from one solution to another.
        /// </summary>
        event EventHandler<WorkspaceExecutionTargetChangedEventArgs> WorkspaceExecutionTargetChanged;

        /// <summary>
        /// Occurs when a new solution is opened.
        /// </summary>
        event EventHandler<SolutionOpenedEventArgs> SolutionOpened;

        /// <summary>
        /// Occurs when a solution is closed.
        /// </summary>
        event EventHandler<SolutionClosedEventArgs> SolutionClosed;

        /// <summary>
        /// Occurs when a project is added to a solution.
        /// </summary>
        event EventHandler<ProjectAddedEventArgs> ProjectAdded;

        /// <summary>
        /// Occurs when a project is removed from a solution.
        /// </summary>
        event EventHandler<ProjectRemovedEventArgs> ProjectRemoved;

        /// <summary>
        /// Occurs when a project is renamed.
        /// </summary>
        event EventHandler<ProjectRenamedEventArgs> ProjectRenamed;

        /// <summary>
        /// Occurs when a reference is added to a project.
        /// </summary>
        event EventHandler<ProjectReferenceAddedEventArgs> ProjectReferenceAdded;

        /// <summary>
        /// Occurs when a reference is removed from a project.
        /// </summary>
        event EventHandler<ProjectReferenceRemovedEventArgs> ProjectReferenceRemoved;

        /// <summary>
        /// Occurs when a file is added to a project.
        /// </summary>
        event EventHandler<FilesEventArgs> FilesAddedToProject;

        /// <summary>
        /// Occurs when a file is removed from a project.
        /// </summary>
        event EventHandler<FilesEventArgs> FilesRemovedFromProject;

        /// <summary>
        /// Occurs when a project file is changed.
        /// </summary>
        event EventHandler<FilesEventArgs> FilesChanged;

        /// <summary>
        /// Occurs when a project file is renamed.
        /// </summary>
        event EventHandler<FilesRenamedEventArgs> FilesRenamed;
    }
}