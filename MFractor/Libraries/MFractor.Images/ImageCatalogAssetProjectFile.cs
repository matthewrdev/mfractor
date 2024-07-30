using System.Collections.Generic;
using System.IO;
using System.Linq;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    /// <summary>
    /// Image catalog asset project file.
    /// </summary>
    public class ImageCatalogAssetProjectFile : IProjectFile
    {
        public Project CompilationProject => Catalog.CompilationProject;

        public Document CompilationDocument => null;

        public string FilePath { get; }

        public string VirtualPath { get; }

        public FileInfo FileInfo { get; }

        public string Name { get; }

        public string Extension => Path.GetExtension(Name);

        public IReadOnlyList<string> ProjectFolders { get; }

        public string BuildAction { get; }

        public IProjectFile Catalog { get; }

        public bool IsLink => Catalog.IsLink;

        public string ResourceId => Catalog.ResourceId;

        public ImageCatalogAssetProjectFile(string filePath, IProjectFile catalog)
        {
            FilePath = filePath;

            FileInfo = new FileInfo(filePath);

            Name = Path.GetFileName(FileInfo.Name);

            var path = Path.GetDirectoryName(catalog.VirtualPath);

            VirtualPath = path + "/" + Name;

            Catalog = catalog;

            var folders = catalog.ProjectFolders.ToList();

            folders.Add(catalog.Name);

            ProjectFolders = folders;
        }
    }
}
