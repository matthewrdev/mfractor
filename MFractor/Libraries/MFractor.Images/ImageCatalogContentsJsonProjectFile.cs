using System.Collections.Generic;
using System.IO;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    /// <summary>
    /// Image catalog asset project file.
    /// </summary>
    public class ImageCatalogContentsJsonProjectFile : IProjectFile
    {
        public Project CompilationProject { get; }

        public Document CompilationDocument => null;

        public string FilePath { get; }

        public string VirtualPath { get; }

        public FileInfo FileInfo { get; }

        public string Name { get; }

        public string Extension => Path.GetExtension(Name);

        public IReadOnlyList<string> ProjectFolders { get; }

        public string BuildAction { get; }

        public bool IsLink => false;

        public string ResourceId => string.Empty;

        public ImageCatalogContentsJsonProjectFile(string filePath, Project project)
        {
            FilePath = filePath;

            FileInfo = new FileInfo(filePath);

            Name = Path.GetFileName(FileInfo.Name);

            VirtualFilePathHelper.ExtractVirtualPath(project.FilePath, filePath, out var virtualPath, out var projectFolders);

            VirtualPath = virtualPath;
            ProjectFolders = projectFolders;
        }
    }
}
