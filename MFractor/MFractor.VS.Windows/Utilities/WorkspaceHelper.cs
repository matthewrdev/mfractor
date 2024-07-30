using System.Linq;
using EnvDTE;
using EnvDTE80;
using MFractor.IOC;
using MFractor.Workspace;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows.Utilities
{
    public static class WorkspaceHelper
    {
        static IWorkspaceService WorkspaceService => Resolver.Resolve<IWorkspaceService>();

        static DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        public static Microsoft.CodeAnalysis.Project ToCompilationProject(this EnvDTE.Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectFilePath = project.FullName;

            if (WorkspaceService.CurrentWorkspace == null
                || WorkspaceService.CurrentWorkspace.CurrentSolution == null)
            {
                return default;
            }

            return WorkspaceService.CurrentWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFilePath);
        }

        public static Microsoft.CodeAnalysis.Solution ToCompilationSolution(this EnvDTE.Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solutionFilePath = solution.FullName;

            if (WorkspaceService.CurrentWorkspace == null
                || WorkspaceService.CurrentWorkspace.CurrentSolution == null
                || WorkspaceService.CurrentWorkspace.CurrentSolution.FilePath != solutionFilePath)
            {
                return default;
            }

            return WorkspaceService.CurrentWorkspace.CurrentSolution;
        }
        public static Project ToIdeProject(this Microsoft.CodeAnalysis.Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (DTE.Solution == null 
                || DTE.Solution.Projects == null
                || DTE.Solution.Projects.Count == 0)
            {
                return null;
            }

            return DTE.Solution.Projects
                .Cast<Project>()
                .FirstOrDefault(p => p.Name == project.Name);
        }

        public static Project ToIdeProject(this ProjectIdentifier project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (DTE.Solution == null
                || DTE.Solution.Projects == null
                || DTE.Solution.Projects.Count == 0)
            {
                return null;
            }

            return DTE.Solution.Projects
                .Cast<Project>()
                .FirstOrDefault(p => p.Name == project.Name);
        }

        public static Solution ToIdeSolution(this Microsoft.CodeAnalysis.Solution solution)
        {
            var solutionFilePath = solution.FilePath;

            if (DTE.Solution == null
                || DTE.Solution.FullName != solutionFilePath)
            {
                return null;
            }

            return DTE.Solution;
        }
    }
}
