using System;
using System.Collections.Generic;
using EnvDTE;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows.Services
{
    class IdeProjectFolder : IProjectFolder
    {
        readonly ProjectItem projectItem;

        public IdeProjectFolder(ProjectItem folder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            projectItem = folder ?? throw new ArgumentNullException(nameof(folder));
            if (projectItem.Kind != Constants.vsProjectItemKindPhysicalFolder)
            {
                throw new ArgumentException(nameof(folder), "The project item is not a Folder Kind.");
            }

            Name = projectItem.Name;
            if (projectItem.FileCount > 0 && projectItem.FileNames[0] != null)
            {
                Path = projectItem.FileNames[0];
            }

            // TODO: Build virtual folder path.
        }

        public Microsoft.CodeAnalysis.Project Project => projectItem.ContainingProject.ToCompilationProject();

        public string Path { get; }

        public string VirtualPath { get; }

        public string Name { get; }
    }
}
