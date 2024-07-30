using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.VS.Mac.Services
{
    class IdeProjectFile : IProjectFile
    {
        public IdeProjectFile(MonoDevelop.Projects.ProjectFile projectFile, Project project)
        {
            ProjectFile = projectFile;
            CompilationProject = project;
            FileInfo = new FileInfo(projectFile.FilePath);

            string virtualPath = projectFile.ProjectVirtualPath;
            virtualPath = virtualPath.Replace(FileInfo.Name, "");

            if (!string.IsNullOrEmpty(virtualPath))
            {
                if (virtualPath.Last() == Path.DirectorySeparatorChar)
                {
                    virtualPath = virtualPath.Remove(virtualPath.Length - 1);
                }

                ProjectFolders = virtualPath.Split(Path.DirectorySeparatorChar).ToList();
            }
            else
            {
                ProjectFolders = new List<string>();
            }
        }

        public Project CompilationProject { get; }

        public Document CompilationDocument => CompilationProject.Documents.FirstOrDefault(d => d.FilePath == FilePath);

        public string FilePath => ProjectFile.FilePath;

        public string BuildAction => ProjectFile.BuildAction;

        public MonoDevelop.Projects.ProjectFile ProjectFile { get; }

        public string Name => Path.GetFileName(ProjectFile.FilePath);

        public FileInfo FileInfo { get; }

        public IReadOnlyList<string> ProjectFolders { get; }

        public string VirtualPath => ProjectFile.ProjectVirtualPath;

        public bool IsLink => ProjectFile.IsLink;

        public string Extension => Path.GetExtension(Name);

        public string ResourceId => ProjectFile.ResourceId;
    }
}
