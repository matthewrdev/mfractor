using System;
using System.Collections.Generic;using System.ComponentModel.Composition;using System.IO;using System.Linq;using MFractor.VS.Windows.WorkspaceModel;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;using Microsoft.VisualStudio.Shell;using Project = Microsoft.CodeAnalysis.Project;
namespace MFractor.VS.Windows.Services{    [PartCreationPolicy(CreationPolicy.Shared)]    [Export(typeof(IProjectService))]    class ProjectService : IProjectService    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IWorkspaceShadowModel> workspaceShadowModel;
        public IWorkspaceShadowModel WorkspaceModel => workspaceShadowModel.Value;

        [ImportingConstructor]
        public ProjectService(Lazy<IWorkspaceService> workspaceService, 
                              Lazy<IWorkspaceShadowModel> workspaceShadowModel,
                              Lazy<IDialogsService> dialogsService)
        {
            this.workspaceShadowModel = workspaceShadowModel;
            this.dialogsService = dialogsService;
        }
        public string GetDefaultNamespace(Project project)        {            var guid = GetProjectGuid(project);            var ideProject = WorkspaceModel.GetProjectByGuid(guid);            if (ideProject == null)
            {
                return string.Empty;
            }            return ideProject.DefaultNamespace;        }

        public string GetDefaultNamespace(ProjectIdentifier projectIdentifier)        {            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);            if (ideProject == null)
            {
                return string.Empty;
            }            return ideProject.DefaultNamespace;
        }        public string GetDefaultNamespace(Project project, string filePath)        {            return GetDefaultNamespace(project);        }        public Project GetProject(string guid)        {            var ideProject = WorkspaceModel.GetProjectByGuid(guid);            if (ideProject == null)
            {
                return default;
            }            return ideProject.CompilationProject;        }        public Project GetProject(ProjectIdentifier projectIdentifier)        {            return GetProject(projectIdentifier.Guid);        }        public IProjectFile GetProjectFileWithFilePath(ProjectIdentifier projectIdentifier, string filePath)        {
            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);

            if (ideProject == null)
            {
                return default;
            }

            return ideProject.GetProjectFile(filePath);        }        public IReadOnlyList<IProjectFile> GetProjectFiles(Project project)        {            var guid = GetProjectGuid(project);

            var ideProject = WorkspaceModel.GetProjectByGuid(guid);

            if (ideProject == null)
            {
                return Array.Empty<IProjectFile>();
            }
            return ideProject.ProjectFiles.ToList();        }        public string GetProjectGuid(Project project)        {
            if (project == null)
            {
                return string.Empty;
            }

            var ideProject = WorkspaceModel.GetProjectByName(project.Name);            if (ideProject == null)
            {
                return string.Empty;
            }            return ideProject.Guid;        }        public IReadOnlyList<IProjectFile> GetProjectFilesWithExtension(Project project, string extension, StringComparison stringComparison = StringComparison.Ordinal)        {
            if (extension is null)
            {
                return Array.Empty<IProjectFile>();
            }

            return GetProjectFiles(project).Where(pf => extension.Equals(pf.Extension, stringComparison)).ToList();        }        public IReadOnlyList<IProjectFile> GetProjectFilesWithBuildAction(Project project, string buildAction, StringComparison stringComparison = StringComparison.Ordinal)        {
            if (buildAction is null)
            {
                return Array.Empty<IProjectFile>();
            }

            return GetProjectFiles(project).Where(pf => buildAction.Equals(pf.BuildAction, stringComparison)).ToList();        }        public bool IsSharedAssetsProject(ProjectIdentifier projectIdentifier)        {            return false;        }        public Compilation GetCompilation(ProjectIdentifier projectIdentifier, bool resolveIfSharedAssetsProject = true)        {            var project = GetProject(projectIdentifier);            if (project == null)
            {
                return null;
            }            project.TryGetCompilation(out var compilation);            return compilation;        }        public IReadOnlyList<IProjectFile> GetProjectFiles(ProjectIdentifier projectIdentifier)        {
            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);

            if (ideProject == null)
            {
                return Array.Empty<IProjectFile>();
            }
            return ideProject.ProjectFiles.ToList();        }

        public void DeleteProjectFile(string filePath, Project project, bool confirmDeletion = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion("Are you sure you want to delete " + Path.GetFileName(filePath) + "?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            var guid = GetProjectGuid(project);

            var ideProject = WorkspaceModel.GetProjectByGuid(guid);

            if (ideProject != null)
            {
                var projectFile = ideProject.GetProjectFile(filePath);

                if (projectFile is IdeProjectFile ideProjectFile)
                {
                    ideProjectFile.ProjectItem.Delete();
                }
                else
                {
                    if (File.Exists(projectFile.FilePath))
                    {
                        File.Delete(projectFile.FilePath);
                    }
                }

                ideProject.Project.Save();
            }
        }

        public void DeleteProjectFile(IProjectFile projectFile, bool confirmDeletion = true)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning("DeleteProjectFile was called from a non-UI thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion("Are you sure you want to delete " + projectFile.Name + "?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            if (projectFile is IdeProjectFile ideProjectFile)
            {
                var project = ideProjectFile.ProjectItem.ContainingProject;
                ideProjectFile.ProjectItem.Delete();
                project.Save();
            }
        }

        public void DeleteProjectFiles(IReadOnlyList<string> filePaths, Project project, bool confirmDeletion = true)
        {            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning("DeleteProjectFiles was called from a non-UI thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion($"Are you sure you want to {filePaths.Count()} files from" + project.Name + "?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            var guid = GetProjectGuid(project);

            var ideProject = WorkspaceModel.GetProjectByGuid(guid);


            if (ideProject != null)
            {
                foreach (var filePath in filePaths)
                {
                    var projectFile = ideProject.GetProjectFile(filePath);

                    if (projectFile is IdeProjectFile ideProjectFile)
                    {
                        ideProjectFile.ProjectItem.Delete();
                    }
                    else
                    {
                        if (File.Exists(projectFile.FilePath))
                        {
                            File.Delete(projectFile.FilePath);
                        }
                    }
                }

                ideProject.Project.Save();
            }
        }

        public void DeleteProjectFiles(IReadOnlyList<IProjectFile> projectFiles, bool confirmDeletion = true)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning("DeleteProjectFiles was called from a non-UI thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion($"Are you sure you want to delete {projectFiles.Count()} files?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            var ideProjectFiles = projectFiles.Cast<IdeProjectFile>();

            var projects = new HashSet<EnvDTE.Project>();

            foreach (var projectFile in ideProjectFiles)
            {
                projects.Add(projectFile.ProjectItem.ContainingProject);
                projectFile.ProjectItem.Delete();
            }

            foreach (var project in projects)
            {
                project.Save();
            }
        }        public string GetProjectPath(ProjectIdentifier projectIdentifier)        {
            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);

            if (ideProject == null)
            {
                return default;
            }

            return ideProject.FilePath;        }

        public IProjectFile FindProjectFile(Project project, Func<string, bool> searchFunc)
        {
            if (searchFunc is null)
            {
                return default;
            }

            var guid = GetProjectGuid(project);

            var ideProject = WorkspaceModel.GetProjectByGuid(guid);

            if (ideProject == null)
            {
                return default;
            }

            return ideProject.ProjectFiles.FirstOrDefault(pf => searchFunc(pf.FilePath));
        }

        public IProjectFile FindProjectFile(ProjectIdentifier projectIdentifier, Func<string, bool> searchFunc)
        {
            if (searchFunc is null)
            {
                return default;
            }

            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);

            if (ideProject == null)
            {
                return default;
            }

            return ideProject.ProjectFiles.FirstOrDefault(pf => searchFunc(pf.FilePath));
        }

        public IProjectFile GetProjectFileWithFilePath(Project project, string filePath)
        {
            var guid = GetProjectGuid(project);

            var ideProject = WorkspaceModel.GetProjectByGuid(guid);

            if (ideProject == null)
            {
                return default;
            }

            return ideProject.GetProjectFile(filePath);
        }

        public IReadOnlyList<IProjectFile> GetProjectFiles(Project project, IReadOnlyList<string> projectFolders)
        {
            return Array.Empty<IProjectFile>();
        }
    }}