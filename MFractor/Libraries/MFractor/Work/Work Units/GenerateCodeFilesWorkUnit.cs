using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    public delegate IReadOnlyList<IWorkUnit> GenerateCodeFilesDelegate(GenerateCodeFilesResult result);

    public class GenerateCodeFilesResult
    {
        public string Name { get; }

        public string FolderPath { get; }

        public Project SelectedProject => SelectedProjects.FirstOrDefault();

        public IReadOnlyList<Project> SelectedProjects { get; }

        public ProjectSelectorMode ProjectSelectorMode { get; }

        public GenerateCodeFilesResult(string name,
                                       string folderPath,
                                       IReadOnlyList<Project> selectedProjects,
                                       ProjectSelectorMode projectSelectorMode)
        {
            Name = name;
            FolderPath = folderPath;
            SelectedProjects = selectedProjects ?? new List<Project>();
            ProjectSelectorMode = projectSelectorMode;
        }
    }

    /// <summary>
    /// Launches MFractors code generation window.
    /// </summary>
    public class GenerateCodeFilesWorkUnit : WorkUnit
    {
        public GenerateCodeFilesWorkUnit(string defaultName,
                                         Project defaultProject,
                                         IEnumerable<Project> projects,
                                         string folderPath,
                                         string title,
                                         string message,
                                         string helpUrl,
                                         ProjectSelectorMode projectSelectorMode,
                                         GenerateCodeFilesDelegate generateCodeFilesDelegate)
        {
            Projects = (projects ?? Enumerable.Empty<Project>()).ToList();
            Name = defaultName;
            DefaultProject = defaultProject ?? projects.First();
            FolderPath = folderPath;
            Title = title;
            Message = message;
            HelpUrl = helpUrl;
            ProjectSelectorMode = projectSelectorMode;
            GenerateCodeFilesDelegate = generateCodeFilesDelegate ?? throw new ArgumentNullException(nameof(generateCodeFilesDelegate));
        }

        public string Name { get; }

        public bool IsNameEditable { get; set; } = true;

        public Project DefaultProject { get; }

        public IReadOnlyList<Project> Projects { get; }

        public string FolderPath { get; }

        public bool IsFolderPathEditiable { get; set; } = true;

        public string Title { get; }

        public string Message { get; }

        public string HelpUrl { get; }

        public ProjectSelectorMode ProjectSelectorMode { get; }

        public bool RequiresLicense { get; set; } = false;

        public GenerateCodeFilesDelegate GenerateCodeFilesDelegate { get; }
    }
}
