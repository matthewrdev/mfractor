using System;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.WorkUnits
{
    /// <summary>
    /// A <see cref="IWorkUnit"/> that locates the given <see cref="FilePath"/> in the <see cref="ProjectIdentifier"/> and selects it in the IDEs project pad.
    /// </summary>
    public class SelectFileInProjectPadWorkUnit : WorkUnit
    {
        public ProjectIdentifier ProjectIdentifier { get; }

        public string FilePath { get; }

        /// <summary>
        /// When selecting the file, should MFractor try to find this file in any linked shared projects?
        /// </summary>
        /// <value><c>true</c> if infer when in shared project; otherwise, <c>false</c>.</value>
        public bool InferWhenInSharedProject { get; set; } = true;

        public SelectFileInProjectPadWorkUnit(IProjectFile projectFile)
        {
            if (projectFile == null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            ProjectIdentifier = projectFile.CompilationProject.GetIdentifier();
            FilePath = projectFile.FilePath;
        }

        public SelectFileInProjectPadWorkUnit(Project project, string filePath)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            ProjectIdentifier = project.GetIdentifier();
            FilePath = filePath;
        }

        public SelectFileInProjectPadWorkUnit(ProjectIdentifier projectIdentifier, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            ProjectIdentifier = projectIdentifier;
            FilePath = filePath;
        }
    }
}
