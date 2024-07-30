using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities
{
    /// <summary>
    /// A colleciton of helper extension methods for determining the platform a project targets.
    /// </summary>
    public static class ProjectAndSolutionExtensions
    {
        /// <summary>
        /// Gets the targetted <see cref="PlatformFramework"/> for this project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static PlatformFramework GetPlatform(this Project project)
        {
            if (project.IsAndroidProject())
            {
                return PlatformFramework.Android;
            }
            else if (project.IsIOSProject())
            {
                return PlatformFramework.iOS;
            }
            else if (project.IsTVOs())
            {
                return PlatformFramework.TVOS;
            }
            else if (project.IsWatchOS())
            {
                return PlatformFramework.WatchOS;
            }
            else if (project.IsMacProject())
            {
                return PlatformFramework.MacOS;
            }
            else if (project.IsUWPProject())
            {
                return PlatformFramework.UWP;
            }

            return PlatformFramework.Unknown;
        }

        /// <summary>
        /// Gets the iOS and Android projects in the solution.
        /// </summary>
        /// <returns>The mobile projects.</returns>
        /// <param name="solution">Solution.</param>
        public static List<Project> GetMobileProjects(this Solution solution)
        {
            if (solution == null)
            {
                return new List<Project>();
            }

            var result = new List<Project>();

            foreach (var project in solution.Projects)
            {
                if (project.IsAppleUnifiedProject()
                    || project.IsAndroidProject()
                    || project.IsUWPProject()
                    || project.IsMauiProject())
                {
                    result.Add(project);
                }

                // TODO: Is MAUI project?
            }

            return result;
        }

        public static IReadOnlyList<Project> GetDependentMobileProjects(this Project project)
        {
            var iosProjects = project.GetDependentAppleUnifiedProjects();

            var androidProjects = project.GetDependentAndroidProjects();

            var uwpProjects = project.GetDependentUWPProjects();

            var mauiProjects = project.GetDependentMauiProjects();

            if (!iosProjects.Any()
                && !androidProjects.Any()
                && !uwpProjects.Any()
                && !mauiProjects.Any())
            {
                return new List<Project>();
            }

            var allProjects = new List<Project>();
            if (iosProjects.Any())
            {
                allProjects.AddRange(iosProjects);
            }

            if (androidProjects.Any())
            {
                allProjects.AddRange(androidProjects);
            }

            if (uwpProjects.Any())
            {
                allProjects.AddRange(uwpProjects);
            }

            if (mauiProjects.Any())
            {
                allProjects.AddRange(mauiProjects);
            }

            return allProjects.Distinct().ToList();
        }

        public static List<Project> GetDependentAndroidProjects(this Project project)
        {
            var solution = project.Solution;

            var projects = solution.Projects.Where(p => p.IsAndroidProject())
                                     .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id))
                                     .ToList();

            if (project.IsAndroidProject())
            {
                projects.Add(project);
            }

            return projects;
        }

        public static List<Project> GetDependentAppleUnifiedProjects(this Project project)
        {
            var solution = project.Solution;

            var projects = solution.Projects.Where(p => p.IsAppleUnifiedProject())
                                     .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id))
                                     .ToList();

            if (project.IsAppleUnifiedProject())
            {
                projects.Add(project);
            }

            return projects;
        }

        public static List<Project> GetDependentUWPProjects(this Project project)
        {
            var solution = project.Solution;

            var projects = solution.Projects.Where(p => p.IsUWPProject())
                                     .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id))
                                     .ToList();

            if (project.IsUWPProject())
            {
                projects.Add(project);
            }

            return projects;
        }

        public static List<Project> GetDependentMauiProjects(this Project project)
        {
            var solution = project.Solution;

            var projects = solution.Projects.Where(p => p.IsMauiProject())
                                     .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id))
                                     .ToList();

            if (project.IsMauiProject())
            {
                projects.Add(project);
            }

            return projects;
        }

        /// <summary>
        /// Is the provided <paramref name="project"/> a mobile project type?
        /// </summary>
        /// <returns><c>true</c>, if mobile project was ised, <c>false</c> otherwise.</returns>
        /// <param name="project">Project.</param>
        public static bool IsMobileProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            return project.IsAndroidProject()
                || project.IsAppleUnifiedProject()
                || project.IsUWPProject()
                || project.IsMauiProject();
        }

        /// <summary>
        /// Is this project an Android project?
        /// </summary>
        /// <returns><c>true</c>, if the project targets Android, <c>false</c> otherwise.</returns>
        /// <param name="project">Project.</param>
        public static bool IsAndroidProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("__ANDROID__");
        }

        /// <summary>
        /// Is this project .NET Maui shared style project?
        /// </summary>
        public static bool IsMauiProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            return SymbolHelper.HasAssemblyReference(project, "Microsoft.Maui.Controls");
        }

        /// <summary>
        /// Is this project an iOS or Mac project?
        /// </summary>
        /// <returns><c>true</c>, if the project targets iOS, <c>false</c> otherwise.</returns>
        /// <param name="project">Project.</param>
        public static bool IsAppleUnifiedProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("__IOS__")
                          || options.PreprocessorSymbolNames.Contains("__MACOS__")
                          || options.PreprocessorSymbolNames.Contains("__TVOS__")
                          || options.PreprocessorSymbolNames.Contains("__WATCHOS__");
        }

        /// <summary>
        /// Is the <paramref name="project"/> an iOS project?
        /// </summary>
        /// <returns><c>true</c>, if IOSP roject was ised, <c>false</c> otherwise.</returns>
        /// <param name="project">Project.</param>
        public static bool IsIOSProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("__IOS__");
        }

        /// <summary>
        /// Is the <paramref name="project"/> a UWP project?
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static bool IsUWPProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("WINDOWS_UWP");
        }

        /// <summary>
        /// Is the <paramref name="project"/> a WinUI based project?
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static bool IsWinUIProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("__WINUI__"); // TODO: Check this
        }

        /// <summary>
        /// Is the <paramref name="project"/> a Mac project?
        /// </summary>
        /// <returns><c>true</c>, if mac project was ised, <c>false</c> otherwise.</returns>
        /// <param name="project">Project.</param>
        public static bool IsMacProject(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("__MAC__");
        }

        /// <summary>
        /// Is the <paramref name="project"/> a WatchOS project?
        /// </summary>
        /// <param name="project">Project.</param>
        public static bool IsWatchOS(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("__WATCHOS__");
        }

        /// <summary>
        /// Is the <paramref name="project"/> a TVOs project?
        /// </summary>
        /// <param name="project">Project.</param>
        public static bool IsTVOs(this Project project)
        {
            if (project == null)
            {
                return false;
            }

            var options = project.ParseOptions;
            if (options == null ||
                (options.PreprocessorSymbolNames == null || !options.PreprocessorSymbolNames.Any()))
            {
                return false;
            }

            return options.PreprocessorSymbolNames.Contains("__TVOS__");
        }
    }
}
