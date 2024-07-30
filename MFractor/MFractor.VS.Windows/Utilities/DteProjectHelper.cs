using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MFractor.VS.Windows.Utilities
{
    public class DteProjectItemCollection
    {
        public List<ProjectItem> ProjectItems { get; } = new List<ProjectItem>();
        public List<ProjectItem> ProjectFolders { get; } = new List<ProjectItem>();
    }

    static class DteProjectHelper
    {
        static readonly Logging.ILogger log = Logging.Logger.Create();

        static IVsSolution VsSolution => Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

        public static string CleanNameSpace(string @namespace, bool stripPeriods = true)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                return string.Empty;
            }

            if (stripPeriods)
            {
                @namespace = @namespace.Replace(".", "");
            }

            @namespace = @namespace.Replace(" ", "")
                        .Replace("-", "")
                        .Replace("\\", ".");

            return @namespace;
        }

        public static string GetFilePath(Project project)
        {
            if (project is null)
            {
                return string.Empty;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            var @namespace = project.Name ?? string.Empty;

            try
            {
                var prop = project.Properties.Item("RootNamespace");

                if (prop != null
                    && prop.Value != null
                    && !string.IsNullOrEmpty(prop.Value.ToString()))
                {
                    @namespace = prop.Value.ToString();
                }
            }
            catch { /* CompilationProject doesn't have a root namespace */ }

            var cleanedNamespace = CleanNameSpace(@namespace, stripPeriods: false);

            return cleanedNamespace;
        }

        public static string GetDefaultNamesapce(Project project)
        {
            if (project is null)
            {
                return string.Empty;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            var @namespace = project.Name ?? string.Empty;

            try
            {
                if (project.Properties != null)
                {
                    var prop = project.Properties.Item("RootNamespace");

                    if (prop != null
                        && prop.Value != null
                        && !string.IsNullOrEmpty(prop.Value.ToString()))
                    {
                        @namespace = prop.Value.ToString();
                    }
                }
            }
            catch { /* CompilationProject doesn't have a root namespace */ }

            var cleanedNamespace = CleanNameSpace(@namespace, stripPeriods: false);

            return cleanedNamespace;
        }

        public static string GetProjectGuid(EnvDTE.Project ideProject)
        {
            if (ideProject is null)
            {
                return string.Empty;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            VsSolution.GetProjectOfUniqueName(ideProject.UniqueName, out var projectHierarchy);

            if (projectHierarchy != null)
            {
                VsSolution.GetGuidOfProject(projectHierarchy, out var projectGuid);
                var guid = projectGuid.ToString();

                return guid;
            }

            return string.Empty;
        }

        public static IEnumerable<ProjectItem> GetProjectFiles(EnvDTE.ProjectItem folder)
        {
            if (folder is null)
            {
                return Enumerable.Empty<ProjectItem>();
            }

            var files = new List<ProjectItem>();
            var folderName = folder.Name;

            var projectName = folder.ContainingProject.Name;
            // Traverse folders recursively to find a file that returns from the predicate
            foreach (var projectItem in folder.ProjectItems.Cast<ProjectItem>())
            {
                var kind = projectItem.Kind;
                var itemName = projectItem.Name;
                if (kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    var result = GetProjectFiles(projectItem);
                    if (result != null)
                    {
                        files.AddRange(result);
                    }
                }
                else
                {
                    files.Add(projectItem);

                    if (projectItem.ProjectItems.Count > 0)
                    {
                        var result = GetProjectFiles(projectItem);
                        if (result != null)
                        {
                            files.AddRange(result);
                        }
                    }
                }
            }

            return files;
        }

        public static string GetProjectItemFilePath(ProjectItem projectItem)
        {
            if (projectItem is null)
            {
                return string.Empty;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            try

            {
                // TODO: Check if there's a possible case with a ProjectItem with no Filenames associated
                if (projectItem.FileCount > 0 && projectItem.FileNames[0] != null)
                {
                    return projectItem.FileNames[0];
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return string.Empty;
        }


        public static string GetProjectItemBuildAction(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (projectItem.Properties != null && projectItem.Properties.Count > 0)
                {
                    var itemType = projectItem.Properties.Item("ItemType");

                    if (itemType != null && itemType.Value != null)
                    {
                        return (string)itemType.Value;
                    }
                }
            }
            catch (ArgumentException)
            {
                // ItemType not found.
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return string.Empty;
        }

        public static IEnumerable<Project> GetProjects(EnvDTE.Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projects = new List<Project>();

            foreach (var item in solution.Projects)
            {
                var project = item as Project;

                if (project is null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    var innerProjects = GetSolutionFolderProjects(project);
                    projects.AddRange(innerProjects);
                }
                else
                {
                    projects.Add(project);
                }
            }

            return projects;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectName = project.Name;

            if (project?.ProjectItems == null)
            {
                return Enumerable.Empty<Project>();
            }

            var projects = new List<Project>();
            var y = project.ProjectItems.Count;
            for (var i = 1; i <= y; i++)
            {
                var innerItem = project.ProjectItems.Item(i);
                var name = innerItem.Name;

                if (IsFolderKind(innerItem.Kind))
                {
                    projects.AddRange(GetSolutionFolderProjects(innerItem));
                }
                else if (innerItem is Project innerProject)
                {
                    projects.AddRange(GetSolutionFolderProjects(innerProject));
                }

                var subProject = innerItem?.SubProject;
                if (subProject != null)
                {
                    var subProjectName = subProject.Name;
                    projects.Add(subProject);

                    if (IsFolderKind(subProject.Kind))
                    {
                        projects.AddRange(GetSolutionFolderProjects(subProject));
                    }
                }
            }

            return projects;
        }


        public const string Miscellaneous = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";
        public const string SolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        public const string SolutionItem = "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}";
        public const string ProjectFolder = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";

        public static bool IsFolderKind(string itemKind)
        {
            if (string.Equals(itemKind, ProjectFolder, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(itemKind, SolutionFolder, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(itemKind, SolutionItem, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem?.ProjectItems == null)
            {
                return Enumerable.Empty<Project>();
            }

            var projectName = projectItem.Name;

            var projects = new List<Project>();
            var y = projectItem.ProjectItems.Count;
            for (var i = 1; i <= y; i++)
            {
                var innerItem = projectItem.ProjectItems.Item(i);

                if (IsFolderKind(innerItem.Kind))
                { 
                    projects.AddRange(GetSolutionFolderProjects(innerItem));
                }
                else if (innerItem is Project project)
                {
                    projects.Add(project);
                }

                var subProject = innerItem?.SubProject;
                if (subProject != null)
                {
                    projects.Add(subProject);
                }
            }

            return projects;
        }

        public static IEnumerable<Project> GetProjects(EnvDTE.Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projects = new List<Project>();

            foreach (var item in project.ProjectItems)
            {
                if (item is Project pp)
                {
                    projects.Add(pp);

                    var innerProjects = GetProjects(pp);

                    if (innerProjects != null && innerProjects.Any())
                    {
                        projects.AddRange(innerProjects);
                    }
                }
            }

            return projects;
        }

        public static IEnumerable<ProjectItem> GetProjectItems(EnvDTE.Project ideProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var files = new List<ProjectItem>();

            var projectName = ideProject.Name;
            foreach (var item in ideProject.ProjectItems)
            {
                var projectItem = item as ProjectItem;
                if (projectItem == null)
                {
                    continue;
                }

                var kind = projectItem.Kind;
                var name = projectItem.Name;

                if (kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    var result = GetProjectFiles(projectItem);
                    if (result != null)
                    {
                        files.AddRange(result);
                    }
                    continue;
                }
                else
                {
                    files.Add(projectItem);

                    if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
                    {
                        var result = GetProjectFiles(projectItem);
                        if (result != null)
                        {
                            files.AddRange(result);
                        }
                    }
                }
            }

            return files;
        }

    }
}
