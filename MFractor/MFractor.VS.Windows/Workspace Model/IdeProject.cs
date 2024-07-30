using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using MFractor.Images;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace;

namespace MFractor.VS.Windows.WorkspaceModel
{
    /// <summary>
    /// A shadow copy of the DTE project to allow background threaded access to the core data pieces that MFractor uses.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class IdeProject
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly IWorkspaceService workspaceService;

        public IdeProject(IWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
        }

        public IdeSolution Solution { get; private set; }
        public Project Project { get; private set; }
        public string Name { get; private set; }

        public string FilePath { get; private set; }

        public bool IsLoaded => !string.IsNullOrEmpty(FilePath);

        public string Guid { get; private set; }

        public string DefaultNamespace { get; private set; }

        readonly Dictionary<string, IProjectFile> projectFilesMap = new Dictionary<string, IProjectFile>();

        public IReadOnlyList<IProjectFile> ProjectFiles => projectFilesMap.Values.ToList();

        public Microsoft.CodeAnalysis.Project CompilationProject
        {
            get
            {
                var solution = workspaceService.GetSolution(Solution.Name);

                if (solution == null)
                {
                    return null;
                }


                var name = Project.ConfigurationManager.ActiveConfiguration.ConfigurationName;
                var platform = Project.ConfigurationManager.ActiveConfiguration.PlatformName;
                var project = solution.Projects.FirstOrDefault(p => p.Name.StartsWith(Name) && p.CompilationOptions != null);

                return project;
            }
        }

        internal void Update(IdeSolution ideSolution, EnvDTE.Project project)
        {
            Solution = ideSolution;
            Project = project;

            UpdateProperties(project);

            if (IsLoaded)
            {
                UpdateFiles(project);
            }
            else
            {
                projectFilesMap.Clear();
            }
        }

        internal void UpdateFiles(EnvDTE.Project project)
        {
            projectFilesMap.Clear();

            var files = DteProjectHelper.GetProjectItems(project);

            foreach (var file in files)
            {
                AddProjectFile(file);
            }

            UpdateAssetCatalogs(project);
        }

        void UpdateAssetCatalogs(Project project)
        {
            var projectDirectory = Path.GetDirectoryName(project.FullName);

            var assetCatalogs = Directory.GetDirectories(projectDirectory, "*.xcassets");

            if (assetCatalogs != null && assetCatalogs.Any())
            {
                foreach (var catalog in assetCatalogs)
                {
                    var files = ImageAssetHelper.GetAssetCatalogFiles(new DirectoryInfo(catalog));

                    foreach (var file in files)
                    {
                        AddProjectFile(file, "ImageAsset");
                    }
                }
            }
        }

        internal void UpdateFile(EnvDTE.ProjectItem projectItem)
        {
            if (projectItem is null)
            {
                throw new ArgumentNullException(nameof(projectItem));
            }

            var filePath = DteProjectHelper.GetProjectItemFilePath(projectItem);

            if (projectFilesMap.TryGetValue(filePath, out var projectFile)
                && projectFile is IdeProjectFile ideProjectFile)
            {
                ideProjectFile.Update(this, projectItem);
            }
        }

        internal void UpdateProperties(EnvDTE.Project project)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                Name = project.Name;
                FilePath = project.FullName;
            }
            catch (Exception)
            {
            }
            DefaultNamespace = DteProjectHelper.GetDefaultNamesapce(project);
            Guid = DteProjectHelper.GetProjectGuid(project);
        }

        internal void AddProjectFile(FileInfo fileInfo, string buildAction)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            try
            {
                var ideProjectFile = new FileSystemProjectFile();
                projectFilesMap[fileInfo.FullName] = ideProjectFile;
                ideProjectFile.Update(this, fileInfo, buildAction);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        internal void AddProjectFile(EnvDTE.ProjectItem projectItem)
        {
            if (projectItem is null)
            {
                throw new ArgumentNullException(nameof(projectItem));
            }

            var filePath = DteProjectHelper.GetProjectItemFilePath(projectItem);

            try
            {
                var ideProjectFile = new IdeProjectFile();
                projectFilesMap[filePath] = ideProjectFile; 
                ideProjectFile.Update(this, projectItem);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        internal void RemoveProjectFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            if (projectFilesMap.ContainsKey(filePath))
            {
                projectFilesMap.Remove(filePath);
            }
        }

        public IProjectFile GetProjectFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return default;
            }

            if (projectFilesMap.TryGetValue(filePath, out var ideProjectFile))
            {
                return ideProjectFile;
            }

            return default;
        }

        internal void RenameFile(string oldFilePath, string newFilePath)
        {
            if (string.IsNullOrEmpty(oldFilePath))
            {
                throw new ArgumentException("message", nameof(oldFilePath));
            }

            if (string.IsNullOrEmpty(newFilePath))
            {
                throw new ArgumentException("message", nameof(newFilePath));
            }

            if (projectFilesMap.TryGetValue(oldFilePath, out var projectFile))
            {
                if (projectFile is IdeProjectFile ideProjectFile)
                {
                    ideProjectFile.UpdateFilePath(newFilePath);
                }
                else if (projectFile is FileSystemProjectFile fileSystemProjectFile)
                {
                    fileSystemProjectFile.UpdateFilePath(new FileInfo(newFilePath));
                }

                projectFilesMap.Remove(oldFilePath);
                projectFilesMap[newFilePath] = projectFile;
            }
        }
    }
}
