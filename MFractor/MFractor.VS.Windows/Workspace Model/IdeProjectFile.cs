﻿using System;
using System.Collections.Generic;
using MFractor.Utilities;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;

namespace MFractor.VS.Windows.WorkspaceModel
    /// <summary>
    /// A shadow copy of the DTE ProjectItem to allow background threaded access to the core data pieces that MFractor uses.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{VirtualPath}")]
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            ProjectItem = projectItem;

            IdeProject = ideProject;

            UpdateProperties();
            var filePath = DteProjectHelper.GetProjectItemFilePath(projectItem);

            UpdateFilePath(filePath);
        }
        {
        }

        public ProjectItem ProjectItem { get; private set; }
        public IdeProject IdeProject { get; private set; }


        internal void UpdateFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            FilePath = filePath;
            FileInfo = new FileInfo(FilePath);

            VirtualFilePathHelper.ExtractVirtualPath(IdeProject.FilePath, FilePath, out var virtualPath, out var projectFolders);

            VirtualPath = virtualPath;
        }
    }