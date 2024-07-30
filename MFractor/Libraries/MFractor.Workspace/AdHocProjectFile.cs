using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Workspace
{
    /// <summary>
    /// An <see cref="IProjectFile"/> that is 
    /// </summary>
    public class AdHocProjectFile : IProjectFile
    {
        public AdHocProjectFile(Project project, string filePath)
        {
            CompilationProject = project;
            UpdateFilePath(filePath);
        }

        void UpdateFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            FileInfo = new FileInfo(filePath);

            VirtualFilePathHelper.ExtractVirtualPath(CompilationProject.FilePath, FilePath, out var virtualPath, out var projectFolders);

            VirtualPath = virtualPath;
            ProjectFolders = projectFolders;
        }

        public Project CompilationProject { get; }

        public Document CompilationDocument => CompilationProject?.Documents.FirstOrDefault(d => d.FilePath == FilePath);

        public string FilePath => FileInfo.FullName;

        public string VirtualPath { get; private set; }

        public string Extension => FileInfo.Extension;

        public FileInfo FileInfo { get; private set; }

        public string Name => FileInfo.Name;

        public IReadOnlyList<string> ProjectFolders { get; private set; }

        public string BuildAction { get; }

        public bool IsLink { get; }

        public string ResourceId { get; }
    }
}