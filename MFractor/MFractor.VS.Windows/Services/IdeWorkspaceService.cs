using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using EnvDTE;
using EnvDTE80;
using MFractor.Editor;
using MFractor.Utilities;
using MFractor.VS.Windows.Utilities;
using MFractor.VS.Windows.WorkspaceModel;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IWorkspaceService))]
    [Export(typeof(IMutableWorkspaceService))]
    class IdeWorkspaceService : WorkspaceService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

#pragma warning disable IDE0052 // Remove unread private members
        Events events; // Maintained strong reference to prevent GCing.
        Events2 events2; // Maintained strong reference to prevent GCing.
#pragma warning restore IDE0052 // Remove unread private members

        EnvDTE.SolutionEvents solutionEvents;
        DocumentEvents documentEvents;
        ProjectItemsEvents projectItemsEvents;
        ProjectItemsEvents solutionItemsEvents;

        readonly Lazy<IMutableWorkspaceShadowModel> mutableWorkspaceShadowModel;
        IMutableWorkspaceShadowModel WorkspaceModel => mutableWorkspaceShadowModel.Value;

        readonly Lazy<IMutableTextViewService> mutableTextViewService;
        public IMutableTextViewService MutableTextViewService => mutableTextViewService.Value;

        public EnvDTE.Solution DteSolution
        {
            get
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

                return dte?.Solution;
            }
        }


        [Import(typeof(VisualStudioWorkspace))]
        CompilationWorkspace VsWorkspace { get; set; }

        [ImportingConstructor]
        public IdeWorkspaceService(Lazy<IMutableWorkspaceShadowModel> mutableWorkspaceShadowModel,
                                   Lazy<IMutableTextViewService> mutableTextViewService)
        {
            this.mutableWorkspaceShadowModel = mutableWorkspaceShadowModel;
            this.mutableTextViewService = mutableTextViewService;
        }

        public sealed override CompilationWorkspace CurrentWorkspace => VsWorkspace;
            
        public sealed override IEnumerable<CompilationWorkspace> Workspaces => CurrentWorkspace.AsEnumerable();

        string CurrentSolutionName
        {
            get
            {
                var solution = DteSolution;

                if (solution is null)
                {
                    return string.Empty;
                }

                return Path.GetFileName(solution.FileName);
            }
        }

        public void Startup()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

            events = dte.Events;
            events2 = dte.Events as Events2;
            documentEvents = events.DocumentEvents;
            solutionEvents = events.SolutionEvents;
            solutionItemsEvents = events.SolutionItemsEvents;
            projectItemsEvents = events2.ProjectItemsEvents;

            VsWorkspace.WorkspaceChanged += VsWorkspace_WorkspaceChanged;

            solutionEvents.Opened += SolutionEvents_Opened;
            solutionEvents.BeforeClosing += SolutionEvents_BeforeClosing;
            solutionEvents.ProjectAdded += SolutionEvents_ProjectAdded;
            solutionEvents.ProjectRenamed += SolutionEvents_ProjectRenamed;
            solutionEvents.ProjectRemoved += SolutionEvents_ProjectRemoved;
            solutionEvents.Renamed += SolutionEvents_Renamed;

            projectItemsEvents.ItemAdded += ProjectItemsEvents_ItemAdded;
            projectItemsEvents.ItemRemoved += ProjectItemsEvents_ItemRemoved;
            projectItemsEvents.ItemRenamed += ProjectItemsEvents_ItemRenamed;

            documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

            if (dte.Solution != null && !WorkspaceModel.HasSolution(dte.Solution.FileName))
            {
                WorkspaceModel.AddSolution(dte.Solution);
            }
        }

        public void Shutdown()
        {
            VsWorkspace.WorkspaceChanged -= VsWorkspace_WorkspaceChanged;

            if (solutionEvents != null)
            {
                solutionEvents.Opened -= SolutionEvents_Opened;
                solutionEvents.BeforeClosing -= SolutionEvents_BeforeClosing;
                solutionEvents.ProjectAdded -= SolutionEvents_ProjectAdded;
                solutionEvents.ProjectRenamed -= SolutionEvents_ProjectRenamed;
                solutionEvents.ProjectRemoved -= SolutionEvents_ProjectRemoved;
                solutionEvents.Renamed -= SolutionEvents_Renamed;
            }

            if (projectItemsEvents != null)
            {
                projectItemsEvents.ItemAdded -= ProjectItemsEvents_ItemAdded;
                projectItemsEvents.ItemRemoved -= ProjectItemsEvents_ItemRemoved;
                projectItemsEvents.ItemRenamed -= ProjectItemsEvents_ItemRenamed;
            }

            if (documentEvents != null)
            {
                documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;
            }

            events = null;
            events2 = null;
            documentEvents = null;
            solutionEvents = null;
            solutionItemsEvents = null;
            projectItemsEvents = null;
        }

        readonly Dictionary<string, bool> isFirstLoadMap = new Dictionary<string, bool>();


        void SolutionEvents_BeforeClosing()
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(SolutionEvents_BeforeClosing) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var solutionName = CurrentSolutionName;

                WorkspaceModel.RemoveSolution(solutionName);

                if (isFirstLoadMap.ContainsKey(solutionName))
                {
                    isFirstLoadMap.Remove(solutionName);
                }

                base.OnSolutionClosed(CurrentSolutionName);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
        
        void DocumentEvents_DocumentSaved(EnvDTE.Document document)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(DocumentEvents_DocumentSaved) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {

                var filePath = DteProjectHelper.GetProjectItemFilePath(document.ProjectItem);
                var guid = DteProjectHelper.GetProjectGuid(document.ProjectItem.ContainingProject);

                base.OnFileChanged(guid, filePath);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void VsWorkspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            try
            {
                // We are detecting if this is the first the compilation becomes available. If so, we trigger the "proper" solution opened event so all downstream systems that rely on the compilation can monitor changes properly.
                if (e.NewSolution != null)
                {
                    var filePath = e.NewSolution.FilePath;
                    var solutionName = Path.GetFileName(filePath);

                    var isLoaded = isFirstLoadMap.ContainsKey(solutionName);

                    if (!isLoaded)
                    {
                        isFirstLoadMap.Add(solutionName, true);
                        OnSolutionOpened(solutionName);
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void SolutionEvents_Opened()
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(SolutionEvents_Opened) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                // Push this solution into the shadow workspace model. We leave the full opened event to be triggered once the compilation becomes available.
                WorkspaceModel.AddSolution(dte.Solution);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void SolutionEvents_ProjectRemoved(EnvDTE.Project project)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(SolutionEvents_ProjectRemoved) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                var guid = DteProjectHelper.GetProjectGuid(project);
                var solutionName = Path.GetFileName(dte.Solution.FileName);

                WorkspaceModel.RemoveProjectFromSolution(solutionName, guid);

                this.OnProjectRemoved(solutionName, guid);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void ProjectItemsEvents_ItemRenamed(ProjectItem projectItem, string oldName)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(ProjectItemsEvents_ItemRemoved) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {

                if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    return;
                }

                var guid = DteProjectHelper.GetProjectGuid(projectItem.ContainingProject);
                var filePath = DteProjectHelper.GetProjectItemFilePath(projectItem);

                WorkspaceModel.RenameFile(oldName, filePath, guid);

                base.OnFileRenamed(guid, oldName, filePath);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void ProjectItemsEvents_ItemRemoved(ProjectItem projectItem)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(ProjectItemsEvents_ItemRemoved) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {

                if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    return;
                }

                var guid = DteProjectHelper.GetProjectGuid(projectItem.ContainingProject);
                var filePath = DteProjectHelper.GetProjectItemFilePath(projectItem);


                WorkspaceModel.RemoveFileFromProject(filePath, guid);

                base.OnFileRemoved(guid, filePath);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void ProjectItemsEvents_ItemAdded(ProjectItem projectItem)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(ProjectItemsEvents_ItemAdded) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {

                if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    return;
                }

                var guid = DteProjectHelper.GetProjectGuid(projectItem.ContainingProject);
                var filePath = DteProjectHelper.GetProjectItemFilePath(projectItem);

                base.OnFileAdded(guid, filePath);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void SolutionEvents_Renamed(string oldName)
        {
            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                WorkspaceModel.RenameSolution(dte.Solution, oldName);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void SolutionEvents_ProjectRenamed(EnvDTE.Project project, string oldName)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(SolutionEvents_ProjectRenamed) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                var guid = DteProjectHelper.GetProjectGuid(project);

                WorkspaceModel.RenameProject(CurrentSolutionName, guid, project);

                this.OnProjectRenamed(dte.Solution.FullName, DteProjectHelper.GetProjectGuid(project), oldName, project.Name);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void SolutionEvents_ProjectAdded(EnvDTE.Project project)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(SolutionEvents_ProjectAdded) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                var guid = DteProjectHelper.GetProjectGuid(project);

                WorkspaceModel.AddProjectToSolution(CurrentSolutionName, project);

                this.OnProjectAdded(dte.Solution.FullName, DteProjectHelper.GetProjectGuid(project));
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
