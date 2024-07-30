﻿using System;
using System.Collections.Generic;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

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

            {
                return string.Empty;
            }

        public string GetDefaultNamespace(ProjectIdentifier projectIdentifier)
            {
                return string.Empty;
            }
        }
            {
                return default;
            }
            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);

            if (ideProject == null)
            {
                return default;
            }

            return ideProject.GetProjectFile(filePath);

            var ideProject = WorkspaceModel.GetProjectByGuid(guid);

            if (ideProject == null)
            {
                return Array.Empty<IProjectFile>();
            }

            if (project == null)
            {
                return string.Empty;
            }

            var ideProject = WorkspaceModel.GetProjectByName(project.Name);
            {
                return string.Empty;
            }
            if (extension is null)
            {
                return Array.Empty<IProjectFile>();
            }

            return GetProjectFiles(project).Where(pf => extension.Equals(pf.Extension, stringComparison)).ToList();
            if (buildAction is null)
            {
                return Array.Empty<IProjectFile>();
            }

            return GetProjectFiles(project).Where(pf => buildAction.Equals(pf.BuildAction, stringComparison)).ToList();
            {
                return null;
            }
            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);

            if (ideProject == null)
            {
                return Array.Empty<IProjectFile>();
            }


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
        {
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
        }
            var ideProject = WorkspaceModel.GetProjectByGuid(projectIdentifier.Guid);

            if (ideProject == null)
            {
                return default;
            }

            return ideProject.FilePath;

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
    }