using System;
using MFractor.Code.Scaffolding;
using MFractor;
using Microsoft.CodeAnalysis;
using MFractor.Workspace;

namespace MFractor.Code.Scaffolding
{
    public class ScaffoldingContext : IScaffoldingContext
    {
        public ScaffoldingContext(Solution solution)
        {
            Solution = solution ?? throw new ArgumentNullException(nameof(solution));
        }

        public ScaffoldingContext(Project project)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Solution = project.Solution;
        }

        public ScaffoldingContext(IProjectFile projectFile)
        {
            ProjectFile = projectFile ?? throw new ArgumentNullException(nameof(projectFile));
            Solution = projectFile.CompilationProject.Solution;
            Project = projectFile.CompilationProject;
        }

        public Solution Solution { get; }

        public Project Project { get; }

        public IProjectFile ProjectFile { get; }
    }
}