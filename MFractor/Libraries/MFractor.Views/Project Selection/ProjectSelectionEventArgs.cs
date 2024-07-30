using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.ProjectSelection
{
    public class ProjectSelectionEventArgs : EventArgs
    {
        public readonly IReadOnlyList<Project> Projects;

        public ProjectSelectionEventArgs(params Project[] projects)
        {
            Projects = projects?.ToList() ?? new List<Project>();
        }

        public ProjectSelectionEventArgs(IEnumerable<Project> projects)
        {
            Projects = projects?.ToList() ?? new List<Project>();
        }
    }
}
