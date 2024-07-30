using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide;
using MonoDevelop.Ide.TypeSystem;
using MonoDevelop.Projects;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IWorkspaceService))]
    [Export(typeof(IMutableWorkspaceService))]
    class IdeWorkspaceService : WorkspaceService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Dictionary<string, CompilationWorkspace> workspaceMap = new Dictionary<string, Microsoft.CodeAnalysis.Workspace>();
        readonly Dictionary<string, bool> isFirstLoadMap = new Dictionary<string, bool>();

        public sealed override CompilationWorkspace CurrentWorkspace => IdeServices.TypeSystemService.Workspace;

        public sealed override IEnumerable<CompilationWorkspace> Workspaces => IdeServices.TypeSystemService.AllWorkspaces;

        public void Startup()
        {
            IdeApp.Workspace.SolutionLoaded += Workspace_SolutionLoaded;
            IdeApp.Workspace.SolutionUnloaded += Workspace_SolutionUnloaded;
            IdeApp.Workspace.ActiveExecutionTargetChanged += Workspace_ActiveExecutionTargetChanged;
        }

        public void Shutdown()
        {
            IdeApp.Workspace.SolutionLoaded -= Workspace_SolutionLoaded;
            IdeApp.Workspace.SolutionUnloaded -= Workspace_SolutionUnloaded;
            IdeApp.Workspace.ActiveExecutionTargetChanged -= Workspace_ActiveExecutionTargetChanged;
        }

        void Workspace_ActiveExecutionTargetChanged(object sender, System.EventArgs e)
        {
            try
            {
                OnWorkspaceExecutionTargetChanged(this.CurrentWorkspace);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void Workspace_SolutionLoaded(object sender, SolutionEventArgs e)
        {
            try
            {
                var workspace = Workspaces.FirstOrDefault(w => (w as MonoDevelopWorkspace).MonoDevelopSolution.FileName == e.Solution.FileName);

                if (workspace == null)
                {
                    return;
                }

                workspaceMap[e.Solution.FileName] = workspace;

                workspace.WorkspaceChanged += Workspace_WorkspaceChanged;

                e.Solution.SolutionItemAdded += Solution_SolutionItemAdded;
                e.Solution.SolutionItemRemoved += Solution_SolutionItemRemoved;

                foreach (var p in e.Solution.GetAllProjects().OfType<DotNetProject>())
                {
                    BindProjectEvents(p);
                }

                OnWorkspaceAdded(workspace);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void Workspace_SolutionUnloaded(object sender, MonoDevelop.Projects.SolutionEventArgs e)
        {
            try
            {
                if (workspaceMap.ContainsKey(e.Solution.FileName))
                {
                    var workspace = workspaceMap[e.Solution.FileName];

                    workspaceMap.Remove(e.Solution.FileName);

                    if (isFirstLoadMap.ContainsKey(e.Solution.FileName))
                    {
                        isFirstLoadMap.Remove(e.Solution.FileName);
                    }

                    e.Solution.SolutionItemAdded -= Solution_SolutionItemAdded;
                    e.Solution.SolutionItemRemoved -= Solution_SolutionItemRemoved;

                    foreach (var p in e.Solution.GetAllProjects().OfType<DotNetProject>())
                    {
                        UnbindProjectEvents(p);
                    }

                    workspace.WorkspaceChanged -= Workspace_WorkspaceChanged;
                    OnSolutionClosed(Path.GetFileName(e.Solution.FileName));
                    OnWorkspaceRemoved(workspace);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void Solution_SolutionItemAdded(object sender, MonoDevelop.Projects.SolutionItemChangeEventArgs e)
        {
            try
            {
                var project = e.SolutionItem as DotNetProject;

                if (project != null)
                {
                    BindProjectEvents(project);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void Solution_SolutionItemRemoved(object sender, MonoDevelop.Projects.SolutionItemChangeEventArgs e)
        {
            try
            {
                var project = e.SolutionItem as DotNetProject;

                if (project != null)
                {
                    UnbindProjectEvents(project);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void BindProjectEvents(MonoDevelop.Projects.DotNetProject project)
        {
            UnbindProjectEvents(project);

            project.FileRenamedInProject += OnProjectFileRenamed;
            project.FileChangedInProject += OnProjectFileModified;
            project.FileAddedToProject += OnProjectFileAdded;
            project.FileRemovedFromProject += OnProjectFileRemoved;
            project.NameChanged += OnProjectRenamed;
            project.ReferenceAddedToProject += Project_ReferenceAddedToProject;
            project.ReferenceRemovedFromProject += Project_ReferenceRemovedFromProject;
        }

        void UnbindProjectEvents(MonoDevelop.Projects.DotNetProject project)
        {
            project.FileRenamedInProject -= OnProjectFileRenamed;
            project.FileChangedInProject -= OnProjectFileModified;
            project.FileAddedToProject -= OnProjectFileAdded;
            project.FileRemovedFromProject -= OnProjectFileRemoved;
            project.NameChanged -= OnProjectRenamed;
            project.ReferenceAddedToProject += Project_ReferenceAddedToProject;
            project.ReferenceRemovedFromProject += Project_ReferenceRemovedFromProject;
        }

        void OnProjectFileRenamed(object sender, ProjectFileRenamedEventArgs args)
        {
            try
            {
                var renamedFilesChangeSet = new ProjectFileChangeSet<RenamedFile>();
                var addedFilesChangeSet = new ProjectFileChangeSet<string>();
                var removedFilesChangeSet = new ProjectFileChangeSet<string>();

                foreach (var file in args)
                {
                    var projectGuid = SolutionHelper.GetProjectGuid(file.Project);

                    if (!string.IsNullOrEmpty(projectGuid))
                    {
                        renamedFilesChangeSet.AddChange(projectGuid, new RenamedFile(file.OldName, file.NewName));
                        addedFilesChangeSet.AddChange(projectGuid, file.NewName);
                        removedFilesChangeSet.AddChange(projectGuid, file.OldName);
                    }
                }

                OnFilesRemoved(removedFilesChangeSet);
                OnFilesAdded(addedFilesChangeSet);
                OnFilesRenamed(renamedFilesChangeSet);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void OnProjectFileAdded(object sender, ProjectFileEventArgs args)
        {
            try
            {
                var items = GetChangeSet(args);

                OnFilesAdded(items);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void OnProjectFileModified(object sender, ProjectFileEventArgs args)
        {
            try
            {
                var items = GetChangeSet(args);

                OnFilesChanged(items);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        IProjectFileChangeSet<string> GetChangeSet(ProjectFileEventArgs args)
        {
            var changeSet = new ProjectFileChangeSet<string>();
            var items = new Dictionary<string, List<string>>();

            foreach (var file in args)
            {
                var guid = SolutionHelper.GetProjectGuid(file.Project);

                changeSet.AddChange(guid, file.ProjectFile.FilePath);
            }

            return changeSet;
        }

        void OnProjectFileRemoved(object sender, ProjectFileEventArgs args)
        {
            try
            {
                var items = GetChangeSet(args);

                OnFilesRemoved(items);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void OnProjectRenamed(object sender, MonoDevelop.Projects.SolutionItemRenamedEventArgs e)
        {
            try
            {
                var dotNetProject = e.SolutionItem as DotNetProject;
                if (dotNetProject != null)
                {
                    var solutionName = Path.GetFileName(e.Solution.FileName);
                    base.OnProjectRenamed(solutionName, SolutionHelper.GetProjectGuid(dotNetProject), e.OldName, e.NewName);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void Project_ReferenceAddedToProject(object sender, ProjectReferenceEventArgs e)
        {
            try
            {
                var dotNetProject = e.Project as DotNetProject;
                if (dotNetProject != null)
                {
                    var solutionName = Path.GetFileName(e.Project.ParentSolution.FileName);
                    base.OnProjectReferenceAdded(solutionName, SolutionHelper.GetProjectGuid(dotNetProject), e.ProjectReference.ItemName);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void Project_ReferenceRemovedFromProject(object sender, ProjectReferenceEventArgs e)
        {
            try
            {
                var dotNetProject = e.Project as DotNetProject;
                if (dotNetProject != null)
                {
                    var solutionName = Path.GetFileName(e.Project.ParentSolution.FileName);
                    base.OnProjectReferenceRemoved(solutionName, SolutionHelper.GetProjectGuid(dotNetProject), e.ProjectReference.ItemName);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            try
            {
                if (e.Kind == WorkspaceChangeKind.SolutionAdded)
                {
                    var isLoaded = isFirstLoadMap.ContainsKey(e.NewSolution.FilePath);

                    if (!isLoaded)
                    {
                        isFirstLoadMap.Add(e.NewSolution.FilePath, true);
                        OnSolutionOpened(Path.GetFileName(e.NewSolution.FilePath));
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
