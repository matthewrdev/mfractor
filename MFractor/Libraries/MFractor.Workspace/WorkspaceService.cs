using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Workspace
{
	/// <summary>
	/// The base class for a products implementation of <see cref="IWorkspaceService"/>.
	/// </summary>
	public abstract class WorkspaceService : IMutableWorkspaceService
	{
		/// <summary>
		/// Gets the currently active workspace.
		/// </summary>
		/// <value>The current workspace.</value>
		public abstract CompilationWorkspace CurrentWorkspace { get; }

        /// <summary>
        /// Gets all workspaces that are currently active within this product.
        /// </summary>
        /// <value>The workspaces.</value>
        public abstract IEnumerable<CompilationWorkspace> Workspaces { get; }

        /// <summary>
        /// Is there a solution currently loaded?
        /// </summary>
        public bool IsSolutionLoaded => CurrentWorkspace?.CurrentSolution != null;

        /// <summary>
        /// Occurs when a workspace is added.
        /// </summary>
        public event EventHandler<WorkspaceAddedOrRemovedEventArgs> WorkspaceAdded;

        /// <summary>
        /// Occurs when a workspace is removed.
        /// </summary>
		public event EventHandler<WorkspaceAddedOrRemovedEventArgs> WorkspaceRemoved;

        /// <summary>
        /// Occurs when a workspace is removed.
        /// </summary>
        public event EventHandler<WorkspaceExecutionTargetChangedEventArgs> WorkspaceExecutionTargetChanged;

        /// <summary>
        /// Occurs when a solution is opened.
        /// </summary>
		public event EventHandler<SolutionOpenedEventArgs> SolutionOpened;

        /// <summary>
        /// Occurs when a solution is closed.
        /// </summary>
		public event EventHandler<SolutionClosedEventArgs> SolutionClosed;

        /// <summary>
        /// Occurs when a project is added to a solution.
        /// </summary>
		public event EventHandler<ProjectAddedEventArgs> ProjectAdded;

        /// <summary>
        /// Occurs when a project is removed from a solution.
        /// </summary>
		public event EventHandler<ProjectRemovedEventArgs> ProjectRemoved;

        /// <summary>
        /// Occurs when a project is renamed.
        /// </summary>
		public event EventHandler<ProjectRenamedEventArgs> ProjectRenamed;

        /// <summary>
        /// Occurs when a project reference is added to a project.
        /// </summary>
		public event EventHandler<ProjectReferenceAddedEventArgs> ProjectReferenceAdded;

        /// <summary>
        /// Occurs when a project reference is removed from a project.
        /// </summary>
		public event EventHandler<ProjectReferenceRemovedEventArgs> ProjectReferenceRemoved;

        /// <summary>
        /// Occurs when a file is added to a project.
        /// </summary>
		public event EventHandler<FilesEventArgs> FilesAddedToProject;

        /// <summary>
        /// Occurs when a file is removed from a project.
        /// </summary>
		public event EventHandler<FilesEventArgs> FilesRemovedFromProject;

        /// <summary>
        /// Occurs when a file is changed within a project.
        /// </summary>
		public event EventHandler<FilesEventArgs> FilesChanged;

        /// <summary>
        /// Occurs when a file is renamed in a project.
        /// </summary>
		public event EventHandler<FilesRenamedEventArgs> FilesRenamed;

        /// <summary>
        /// Invokes the WorkspaceAdded event.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        protected virtual void OnWorkspaceAdded(CompilationWorkspace workspace)
		{
			WorkspaceAdded?.Invoke(workspace, new WorkspaceAddedOrRemovedEventArgs(workspace));
		}

		/// <summary>
        /// Invokes the <see cref="WorkspaceRemoved"/> event.
		/// </summary>
		/// <param name="workspace">Workspace.</param>
		protected virtual void OnWorkspaceRemoved(CompilationWorkspace workspace)
		{
			WorkspaceRemoved?.Invoke(workspace, new WorkspaceAddedOrRemovedEventArgs(workspace));
		}

        /// <summary>
        /// Invokes the <see cref="WorkspaceExecutionTargetChanged"/> event.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        protected virtual void OnWorkspaceExecutionTargetChanged(CompilationWorkspace workspace)
        {
            WorkspaceExecutionTargetChanged?.Invoke(workspace, new WorkspaceExecutionTargetChangedEventArgs(workspace));
        }

        /// <summary>
        /// Invokes the <see cref="SolutionOpened"/> event.
        /// </summary>
        /// <param name="solutionName">The name of the solution that was opened.</param>
		public virtual void OnSolutionOpened(string solutionName)
		{
            solutionName = Path.GetFileName(solutionName);

			SolutionOpened?.Invoke(this, new SolutionOpenedEventArgs(solutionName));
		}

        /// <summary>
        /// Invokes the <see cref="SolutionClosed"/> event.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
		protected virtual void OnSolutionClosed(string solutionName)
        {
            solutionName = Path.GetFileName(solutionName);

            SolutionClosed?.Invoke(this, new SolutionClosedEventArgs(solutionName));
		}

        /// <summary>
        /// Invokes the <see cref="ProjectAdded"/> event.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        /// <param name="projectGuid">Project GUID.</param>
        protected virtual void OnProjectAdded(string solutionName, string projectGuid)
		{
			ProjectAdded?.Invoke(this, new ProjectAddedEventArgs(solutionName, projectGuid));
		}

        /// <summary>
        /// Invokes the <see cref="ProjectRemoved"/> event.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        /// <param name="projectGuid">Project GUID.</param>
		protected virtual void OnProjectRemoved(string solutionName, string projectGuid)
		{
			ProjectRemoved?.Invoke(this, new ProjectRemovedEventArgs(solutionName, projectGuid));
		}

        /// <summary>
        /// Invokes the <see cref="ProjectRenamed"/> event.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="oldName">Old name.</param>
        /// <param name="newName">New name.</param>
		protected virtual void OnProjectRenamed(string solutionName, string projectGuid, string oldName, string newName)
		{
			ProjectRenamed?.Invoke(this, new ProjectRenamedEventArgs(solutionName, projectGuid, oldName, newName));
		}

        /// <summary>
        /// Invokes the <see cref="ProjectReferenceAdded"/> event.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="referenceName">Reference name.</param>
		protected virtual void OnProjectReferenceAdded(string solutionName, string projectGuid, string referenceName)
		{
			ProjectReferenceAdded?.Invoke(this, new ProjectReferenceAddedEventArgs(solutionName, projectGuid, referenceName));
		}

        /// <summary>
        /// Invokes the <see cref="ProjectReferenceRemoved"/> event.
        /// </summary>
        /// <param name="solutionName">Solution name.</param>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="referenceName">Reference name.</param>
		protected virtual void OnProjectReferenceRemoved(string solutionName, string projectGuid, string referenceName)
		{
			ProjectReferenceRemoved?.Invoke(this, new ProjectReferenceRemovedEventArgs(solutionName, projectGuid, referenceName));
		}

        /// <summary>
        /// Invokes the <see cref="FilesRemovedFromProject"/> event for a single project file
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="filePath">File path.</param>
        protected virtual void OnFileAdded(string guid, string filePath)
        {
            var changeSet = new ProjectFileChangeSet<string>();
            changeSet.AddChange(guid, filePath);

            FilesAddedToProject?.Invoke(this, new FilesEventArgs(changeSet));
        }

        /// <summary>
        /// Invokes the <see cref="FilesAddedToProject"/> event.
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="filePath">File path.</param>
		protected virtual void OnFilesAdded(IProjectFileChangeSet<string> changeSet)
        {
			FilesAddedToProject?.Invoke(this, new FilesEventArgs(changeSet));
		}

        /// <summary>
        /// Invokes the <see cref="FilesRemovedFromProject"/> event for a single project file
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="filePath">File path.</param>
        protected virtual void OnFileRemoved(string guid, string filePath)
        {
            var changeSet = new ProjectFileChangeSet<string>();
            changeSet.AddChange(guid, filePath);

            FilesRemovedFromProject?.Invoke(this, new FilesEventArgs(changeSet));
        }

        /// <summary>
        /// Invokes the <see cref="FileRemovedFromProject"/> event.
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="filePath">File path.</param>
		protected virtual void OnFilesRemoved(IProjectFileChangeSet<string> changeSet)
		{
			FilesRemovedFromProject?.Invoke(this, new FilesEventArgs(changeSet));
		}

        public void NotifyFileChanged(string guid, string filePath)
        {
            OnFileChanged(guid, filePath);
        }

        /// <summary>
        /// Invokes the <see cref="FilesChanged"/> event for a single project file
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="filePath">File path.</param>
        protected virtual void OnFileChanged(string guid, string filePath)
        {
            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var changeSet = new ProjectFileChangeSet<string>();
            changeSet.AddChange(guid, filePath);

            FilesChanged?.Invoke(this, new FilesEventArgs(changeSet));
        }

        /// <summary>
        /// Invokes the <see cref="FilesChanged"/> event.
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="filePath">File path.</param>
		protected virtual void OnFilesChanged(IProjectFileChangeSet<string> changeSet)
        {
			FilesChanged?.Invoke(this, new FilesEventArgs(changeSet));
		}

        /// <summary>
        /// Invokes the <see cref="FilesRenamed"/> event for a single file.
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="oldFilePath">Old file path.</param>
        /// <param name="newFilePath">New file path.</param>
        protected virtual void OnFileRenamed(string guid, string oldFilePath, string newFilePath)
        {
            var changeSet = new ProjectFileChangeSet<RenamedFile>();
            changeSet.AddChange(guid, new RenamedFile(oldFilePath, newFilePath));

            FilesRenamed?.Invoke(this, new FilesRenamedEventArgs(changeSet));
        }

        /// <summary>
        /// Invokes the <see cref="FileRenamed"/> event.
        /// </summary>
        /// <param name="projectGuid">Project GUID.</param>
        /// <param name="oldFilePath">Old file path.</param>
        /// <param name="newFilePath">New file path.</param>
		protected virtual void OnFilesRenamed(IProjectFileChangeSet<RenamedFile> changeSet)
		{
			FilesRenamed?.Invoke(this, new FilesRenamedEventArgs(changeSet));
		}

		/// <summary>
        /// Gets the solution named <paramref name="solutionName"/> or null if no solution with the name <paramref name="solutionName"/> exists.
        /// </summary>
        /// <returns>The solution.</returns>
        /// <param name="solutionName">Solution name.</param>
        public Solution GetSolution(string solutionName)
		{
			return Workspaces?.FirstOrDefault(w =>
			{
				var sn = Path.GetFileName(w.CurrentSolution.FilePath);
				return sn == solutionName;
			})?.CurrentSolution;
		}
    }
}