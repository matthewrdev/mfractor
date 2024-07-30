using System.Collections.Generic;
using System.Linq;
using MFractor.Editor.Utilities;
using MFractor.IOC;
using MFractor.Utilities;
using MFractor.Workspace;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace MFractor.VS.Mac.Utilities
{
    /// <summary>
    /// A helper class for converting to and from Roslyn and MonoDevelop project models.
    /// </summary>
    public static class SolutionHelper
    {
        /// <summary>
        /// Gets the <see cref="MonoDevelop.Projects.Project"/> for the given <paramref name="project"/>.
        /// </summary>
        /// <returns>The IDE project.</returns>
        /// <param name="project">Project.</param>
        public static MonoDevelop.Projects.Project ToIdeProject(this Microsoft.CodeAnalysis.Project project)
        {
            if (project == null || project.Solution == null)
            {
                return null;
            }

            var solution = IdeApp.Workspace?.GetAllSolutions()?.FirstOrDefault((arg) => arg.FileName == project.Solution.FilePath);

            if (solution == null)
            {
                return null;
            }

            return solution.GetAllProjects()?.FirstOrDefault((arg) => arg.FileName == project.FilePath);
        }

        public static MonoDevelop.Projects.Project ToIdeProject(this ProjectIdentifier projectIdentifier)
        {
            var solutions = IdeApp.Workspace.GetAllSolutions();

            MonoDevelop.Projects.Solution solution = null;
            MonoDevelop.Projects.Project project = null;
            foreach (var s in solutions)
            {
                project = s.GetAllProjects().FirstOrDefault(p => GetProjectGuid(p) == projectIdentifier.Guid);
                if (project != null)
                {
                    solution = s;
                    break;
                }
            }

            if (solution == null || project == null)
            {
                return null;
            }

            return project;
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.Project"/> for the <paramref name="ideProject"/>.
        /// </summary>
        /// <returns>The roslyn project.</returns>
        /// <param name="ideProject">IDE project.</param>
        public static Microsoft.CodeAnalysis.Project ToCompilationProject(this MonoDevelop.Projects.Project ideProject)
        {
            if (ideProject == null)
            {
                return null;
            }

            var activeFilter = TextBufferHelper.GetActiveExecutionTargetFilter();
            var workspace = Resolver.Resolve<IWorkspaceService>().CurrentWorkspace;
            if (ideProject is MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject sharedProject)
            {
                var solution = sharedProject.ParentSolution;
                var dotNetProjects = sharedProject.ParentSolution.GetAllProjects().OfType<DotNetProject>();
                List<DotNetProject> candidateProjects = new List<DotNetProject>();

                Project result = null;
                foreach (var dotNetProject in dotNetProjects)
                {
                    foreach (var r in dotNetProject.References.Where(p => p.ReferenceType == MonoDevelop.Projects.ReferenceType.Project))
                    {
                        var shared = r.ResolveProject(solution) as MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject;

                        if (shared != null && shared == sharedProject)
                        {
                            candidateProjects.Add(dotNetProject);
                        }
                    }
                }

                if (!candidateProjects.Any())
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(activeFilter))
                {
                    foreach (var cp in candidateProjects)
                    {
                        var compilationProject = cp.ToCompilationProject();

                        switch (activeFilter)
                        {
                            case "IOS":
                                {
                                    if (compilationProject.IsIOSProject())
                                    {
                                        return compilationProject;
                                    }
                                }
                                break;
                            case "ANDROID":
                                {
                                    if (compilationProject.IsAndroidProject())
                                    {
                                        return compilationProject;
                                    }
                                }
                                break;
                            case "MACOS":
                                {
                                    if (compilationProject.IsMacProject())
                                    {
                                        return compilationProject;
                                    }
                                }
                                break;
                        }
                    }
                }

                // Fallback to first resolveable project.
                ideProject = candidateProjects.FirstOrDefault();
            }

            if (ideProject == null)
            {
                return null;
            }

            var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == ideProject.FileName && p.Name == ideProject.Name);

            return project;
        }

        /// <summary>
        /// Get the project guid for the given <paramref name="project"/>.
        /// </summary>
        /// <returns>The project GUID.</returns>
        /// <param name="project">Project.</param>
        public static string GetProjectGuid(this Project project)
        {
            if (project == null)
            {
                return string.Empty;
            }

            return project.ItemId.Replace("{", "").Replace("}", "");
        }

        /// <summary>
        /// Given the <paramref name="documentName"/> and <paramref name="project"/>, try to resolve the <see cref="MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject"/>.
        /// </summary>
        /// <returns>The resolve owning shared assets project.</returns>
        /// <param name="documentName">Document name.</param>
        /// <param name="project">Project.</param>
        public static MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject TryResolveOwningSharedAssetsProject(string documentName, Project project)
        {
            if (string.IsNullOrEmpty(documentName)
                || project == null
                || project.ParentSolution == null)
            {
                return null;
            }

            var solution = project.ParentSolution;
            var projects = project.ParentSolution.GetAllProjects();
            if (projects.Any(p => p is MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject))
            {
                var dotNetProject = project as MonoDevelop.Projects.DotNetProject;

                foreach (var r in dotNetProject.References.Where(p => p.ReferenceType == MonoDevelop.Projects.ReferenceType.Project))
                {
                    var sharedProject = r.ResolveProject(solution) as MonoDevelop.Projects.SharedAssetsProjects.SharedAssetsProject;

                    if (sharedProject != null)
                    {
                        var isInShared = sharedProject.Items.OfType<MonoDevelop.Projects.ProjectFile>().Any(i => i.FilePath == documentName);
                        if (isInShared)
                        {
                            return sharedProject;
                        }
                    }
                }
            }

            return null;
        }
    }
}
