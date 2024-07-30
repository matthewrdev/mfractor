using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using MFractor.Utilities;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;

namespace MFractor.VS.Windows.WorkspaceModel
{
    /// <summary>
    /// A shadow copy of the DTE ProjectItem to allow background threaded access to the core data pieces that MFractor uses.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{VirtualPath}")]
    public class FileSystemProjectFile : IProjectFile
    {
        public void Update(IdeProject ideProject, FileInfo fileInfo, string buildAction)
        {
            IdeProject = ideProject;
            BuildAction = buildAction;

            UpdateFilePath(fileInfo);
        }


        public Microsoft.CodeAnalysis.Project CompilationProject => IdeProject.CompilationProject;

        public Microsoft.CodeAnalysis.Document CompilationDocument => null;

        public string FilePath { get; private set; }

        public IdeProject IdeProject { get; private set; }

        public string BuildAction { get; private set; }

        public string ProjectGuid { get; private set; }

        public string Name => Path.GetFileName(FilePath);

        public FileInfo FileInfo { get; private set; }

        public IReadOnlyList<string> ProjectFolders { get; private set; }

        public string VirtualPath { get; private set; }

        public string Extension => Path.GetExtension(FilePath);

        public bool IsLink { get; private set; }

        public string ResourceId { get; private set; }

        internal void UpdateFilePath(FileInfo fileInfo)
        {
            FilePath = fileInfo.FullName;
            FileInfo = fileInfo;

            VirtualFilePathHelper.ExtractVirtualPath(IdeProject.FilePath, FilePath, out var virtualPath, out var projectFolders);

            VirtualPath = virtualPath;
            ProjectFolders = projectFolders;
        }
    }
}
