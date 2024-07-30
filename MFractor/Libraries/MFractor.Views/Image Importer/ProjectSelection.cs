using Microsoft.CodeAnalysis;

namespace MFractor.Views.ImageImporter
{
    public class ProjectSelection
    {
        public Project Project { get; }
        public bool SelectedByDefault { get; }

        public ProjectSelection(Project project, bool selectedByDefault)
        {
            Project = project;
            SelectedByDefault = selectedByDefault;
        }
    }
}
