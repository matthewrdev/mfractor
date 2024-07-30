using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MDSharedAssetsProject = MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProjectService))]
    class ProjectService : IProjectService
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public ProjectService(Lazy<IWorkspaceService> workspaceService,
                              Lazy<IDialogsService> dialogsService,
                              Lazy<IDispatcher> dispatcher)
        {
            this.workspaceService = workspaceService;
            this.dialogsService = dialogsService;
            this.dispatcher = dispatcher;
        }

        public string GetDefaultNamespace(Project project)
        {
            if (project == null)
            {
                return string.Empty;
            }

            return GetDefaultNamespace(project.GetIdentifier());
        }

        public string GetDefaultNamespace(ProjectIdentifier projectIdentifier)
        {
            if (projectIdentifier == null)
            {
                return string.Empty;
            }

            var ideProject = projectIdentifier.ToIdeProject();

            if (ideProject == null)
            {
                return string.Empty;
            }

            var props = ideProject.ProjectProperties;

            // Resolve the namespace, considering if the project is in a shared project.
            var namespaceValue = props.GetProperty("RootNamespace")?.UnevaluatedValue;

            if (ideProject is MDSharedAssetsProject sharedProject)
            {
                if (string.IsNullOrEmpty(namespaceValue))
                {
                    namespaceValue = sharedProject.ProjectProperties.GetProperty("Import_RootNamespace").UnevaluatedValue;
                }
            }

            if (string.IsNullOrEmpty(namespaceValue))
            {
                namespaceValue = ideProject.Name;
            }
            else if (namespaceValue.StartsWith("$(MSBuildProjectName"))
            {
                namespaceValue = ideProject.Name;
            }

            return namespaceValue;
        }


        public string GetDefaultNamespace(Project project, string filePath)
        {
            if (project == null)
            {
                return string.Empty;
            }

            var ideProject = project.ToIdeProject();

            if (ideProject == null || string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            var props = ideProject.ProjectProperties;

            // Resolve the namespace, considering if the project is in a shared project.
            var namespaceValue = props.GetProperty("RootNamespace").UnevaluatedValue;

            var sharedProject = SolutionHelper.TryResolveOwningSharedAssetsProject(Path.GetFileName(filePath), ideProject);
            if (sharedProject != null)
            {
                namespaceValue = sharedProject.ProjectProperties.GetProperty("Import_RootNamespace").UnevaluatedValue;
            }

            if (namespaceValue == "$(MSBuildProjectName)")
            {
                namespaceValue = ideProject.Name;
            }

            return namespaceValue;
        }

        public Project GetProject(string guid)
        {
            var workspace = WorkspaceService.CurrentWorkspace;

            return workspace.CurrentSolution.Projects.Where(p => p.SupportsCompilation).FirstOrDefault(p => GetProjectGuid(p) == guid);
        }

        public Project GetProject(ProjectIdentifier projectIdentifier)
        {
            if (projectIdentifier == null)
            {
                return null;
            }

            if (TryGetSharedAssetsProject(projectIdentifier, out var sharedAssetsProject))
            {
                return sharedAssetsProject.ToCompilationProject();
            }

            return GetProject(projectIdentifier.Guid);
        }

        public string GetProjectPath(ProjectIdentifier projectIdentifier)
        {
            if (projectIdentifier == null)
            {
                return null;
            }

            if (TryGetSharedAssetsProject(projectIdentifier, out var sharedAssetsProject))
            {
                return sharedAssetsProject.FileName;
            }

            return GetProject(projectIdentifier.Guid)?.FilePath;
        }

        public IProjectFile FindProjectFile(ProjectIdentifier projectIdentifier, Func<string, bool> searchFunc)
        {
            if (projectIdentifier == null || searchFunc == null)
            {
                return null;
            }

            var ideProject = projectIdentifier.ToIdeProject();

            if (ideProject == null)
            {
                return null;
            }

            var file = ideProject.Files.FirstOrDefault(pf => searchFunc(pf.FilePath));

            if (file == null)
            {
                return null;
            }

            return new IdeProjectFile(file, GetProject(projectIdentifier));
        }

        public IProjectFile FindProjectFile(Project project, Func<string, bool> searchFunc)
        {
            if (project == null)
            {
                return null;
            }

            var ideProject = project.ToIdeProject();

            if (ideProject == null)
            {
                return null;
            }

            var file = ideProject.Files.FirstOrDefault(pf => searchFunc(pf.FilePath));

            if (file == null)
            {
                return null;
            }

            return new IdeProjectFile(file, project);
        }

        public IProjectFile GetProjectFileWithFilePath(Project project, string filePath)
        {
            if (project == null)
            {
                return null;
            }

            var ideProject = project.ToIdeProject();

            if (ideProject == null)
            {
                return null;
            }

            var file = ideProject.GetProjectFile(filePath);

            if (file == null)
            {
                return null;
            }

            return new IdeProjectFile(file, project);
        }

        public IProjectFile GetProjectFileWithFilePath(ProjectIdentifier projectIdentifier, string filePath)
        {
            if (projectIdentifier == null)
            {
                return null;
            }

            var ideProject = projectIdentifier.ToIdeProject();

            if (ideProject == null)
            {
                return null;
            }

            var file = ideProject.GetProjectFile(filePath);

            if (file == null)
            {
                return null;
            }

            return new IdeProjectFile(file, ideProject.ToCompilationProject());
        }

        public IReadOnlyList<IProjectFile> GetProjectFiles(ProjectIdentifier projectIdentifier)
        {
            if (projectIdentifier == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            var ideProject = projectIdentifier.ToIdeProject();

            if (ideProject == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            var project = GetProject(projectIdentifier);

            return ideProject.Files.Select(f => new IdeProjectFile(f, project)).ToList();
        }

        public IReadOnlyList<IProjectFile> GetProjectFiles(Project project, IReadOnlyList<string> projectFolders)
        {
            if (project == null || projectFolders == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            var ideProject = project.ToIdeProject();

            if (ideProject == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            var folderPath = projectFolders.Any() ? string.Join(Path.DirectorySeparatorChar.ToString(), projectFolders) : string.Empty;

            return ideProject.Files.Where(file =>
            {
                if (file.Subtype == MonoDevelop.Projects.Subtype.Directory)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(file.ProjectVirtualPath.ParentDirectory.ToString()))
                {
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        return true;
                    }

                    return false;
                }

                var directory = file.ProjectVirtualPath.ParentDirectory.ToString();

                return directory == folderPath;
            }).Select(file => new IdeProjectFile(file, project))
            .ToList();
        }

        public IReadOnlyList<IProjectFile> GetProjectFiles(Project project)
        {
            if (project == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            return GetProjectFiles(project.GetIdentifier());
        }

        public string GetProjectGuid(Project project)
        {
            return SolutionHelper.GetProjectGuid(project.ToIdeProject());
        }

        public IReadOnlyList<IProjectFile> GetProjectFilesWithExtension(Project project, string extension, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (project == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            var ideProject = project.ToIdeProject();

            if (ideProject == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            return ideProject.Files.Where(file => Path.GetExtension(file.Name).Equals(extension, stringComparison))
                                   .Select(file => new IdeProjectFile(file, project))
                                   .ToList();
        }

        public IReadOnlyList<IProjectFile> GetProjectFilesWithBuildAction(Project project, string buildAction, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (project == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            var ideProject = project.ToIdeProject();

            if (ideProject == null)
            {
                return Array.Empty<IdeProjectFile>();
            }

            return ideProject.Files.Where(file => file.BuildAction.Equals(buildAction, stringComparison))
                                   .Select(file => new IdeProjectFile(file, project))
                                   .ToList();
        }

        public bool IsSharedAssetsProject(ProjectIdentifier projectIdentifier)
        {
            return TryGetSharedAssetsProject(projectIdentifier, out _);
        }

        public bool TryGetSharedAssetsProject(ProjectIdentifier projectIdentifier, out MDSharedAssetsProject sharedAssetsProject)
        {
            sharedAssetsProject = null;
            if (projectIdentifier == null)
            {
                return false;
            }

            var ideProject = projectIdentifier.ToIdeProject();

            if (ideProject == null)
            {
                return false;
            }

            sharedAssetsProject = ideProject as MDSharedAssetsProject;

            return sharedAssetsProject != null;
        }

        public Compilation GetCompilation(ProjectIdentifier projectIdentifier, bool resolveIfSharedAssetsProject = true)
        {
            if (projectIdentifier == null)
            {
                return null;
            }

            Project project = null;
            if (TryGetSharedAssetsProject(projectIdentifier, out var sharedAssetsProject))
            {
                if (resolveIfSharedAssetsProject)
                {
                    project = sharedAssetsProject.ToCompilationProject();
                }
            }
            else
            {
                project = GetProject(projectIdentifier);
            }

            if (project == null)
            {
                return null;
            }

            project.TryGetCompilation(out var compilation);

            return compilation;
        }

        public void DeleteProjectFile(string filePath, Project project, bool confirmDeletion = true)
        {
            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion("Are you sure you want to delete " + Path.GetFileName(filePath) + "?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            var ideProject = project.ToIdeProject();

            if (ideProject != null)
            {
                var ideProjectFile = ideProject.GetProjectFile(filePath);

                if (ideProjectFile != null)
                {
                    ideProject.Files.Remove(ideProjectFile);
                    if (File.Exists(ideProjectFile.FilePath))
                    {
                        File.Delete(ideProjectFile.FilePath);
                    }

                    ideProject.SaveAsync(new MonoDevelop.Core.ProgressMonitor());
                }
            }
        }

        public void DeleteProjectFile(IProjectFile projectFile, bool confirmDeletion = true)
        {
            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion("Are you sure you want to delete " + projectFile.Name + "?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            Dispatcher.InvokeOnMainThread(() =>
            {
                var ideProject = projectFile.CompilationProject.ToIdeProject();

                if (ideProject != null)
                {
                    var ideProjectFile = ideProject.GetProjectFile(projectFile.FilePath);

                    if (ideProjectFile != null)
                    {
                        ideProject.Files.Remove(ideProjectFile);
                        if (File.Exists(ideProjectFile.FilePath))
                        {
                            File.Delete(ideProjectFile.FilePath);
                        }
                        ideProject.SaveAsync(new MonoDevelop.Core.ProgressMonitor());
                    }
                }
            });
        }

        public void DeleteProjectFiles(IReadOnlyList<string> filePaths, Project project, bool confirmDeletion = true)
        {
            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion($"Are you sure you want to {filePaths.Count()} files from" + project.Name + "?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            var ideProject = project.ToIdeProject();

            if (ideProject != null)
            {
                foreach (var filePath in filePaths)
                {
                    var ideProjectFile = ideProject.GetProjectFile(filePath);

                    if (ideProjectFile != null)
                    {
                        ideProject.Files.Remove(ideProjectFile);
                        if (File.Exists(ideProjectFile.FilePath))
                        {
                            File.Delete(ideProjectFile.FilePath);
                        }
                    }
                }

                ideProject.SaveAsync(new MonoDevelop.Core.ProgressMonitor());
            }
        }

        public void DeleteProjectFiles(IReadOnlyList<IProjectFile> projectFiles, bool confirmDeletion = true)
        {
            if (confirmDeletion)
            {
                var confirmation = DialogsService.AskQuestion($"Are you sure you want to delete {projectFiles.Count()} files?", "Yes", "Cancel");
                if (confirmation != "Yes")
                {
                    return;
                }
            }

            var projects = new HashSet<MonoDevelop.Projects.Project>();

            var deleted = new List<IProjectFile>();

            foreach (var projectFile in projectFiles)
            {
                var ideProject = projectFile.CompilationProject.ToIdeProject();

                if (ideProject != null)
                {
                    var ideProjectFile = ideProject.GetProjectFile(projectFile.FilePath);
                    if (ideProjectFile != null)
                    {
                        ideProject.Files.Remove(ideProjectFile);
                        if (File.Exists(ideProjectFile.FilePath))
                        {
                            File.Delete(ideProjectFile.FilePath);
                        }
                        deleted.Add(projectFile);
                    }

                    projects.Add(ideProject);
                }
            }

            foreach (var project in projects)
            {
                using (var monitor = new MonoDevelop.Core.ProgressMonitor())
                {
                    project.SaveAsync(monitor);
                }
            }
        }
    }
}
