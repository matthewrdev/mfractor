using System.Collections.Generic;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;

namespace MFractor.VS.Mac.Services
{
    class IdeProjectFolder : IProjectFolder
    {
        readonly ProjectFolder folder;

        public IdeProjectFolder(ProjectFolder folder)
        {
            this.folder = folder ?? throw new System.ArgumentNullException(nameof(folder));

            var path = new List<string>
            {
                folder.Name
            };

            var parent = folder.Parent;

            while (parent is ProjectFolder)
            {
                var parentFolder = parent as ProjectFolder;
                path.Insert(0, parentFolder.Name);
                parent = parentFolder.Parent;
            }

            VirtualPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), path);
        }

        public Project Project => folder.Project.ToCompilationProject();

        public string Path => folder.Path;

        public string VirtualPath { get; }

        public string Name => folder.Name;
    }
}