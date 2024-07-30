using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace;

namespace MFractor.VS.Windows.WorkspaceModel
{
    /// <summary>
    /// A shadow copy of the DTE solution to allow background threaded access to the core data pieces that MFractor uses.
    /// </summary>
    public class IdeSolution
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly IWorkspaceService workspaceService;

        public string FilePath { get; private set; }

        public string Name { get; private set; }

        readonly Dictionary<string, IdeProject> projectsMap = new Dictionary<string, IdeProject>();

        public IEnumerable<IdeProject> Projects => projectsMap.Values;

        internal IdeSolution(IWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
        }

        internal void Update(EnvDTE.Solution solution)
        {
            UpdateProperties(solution);
            UpdateProjects(solution);
        }

        internal void UpdateProperties(EnvDTE.Solution solution)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            Name = Path.GetFileName(solution.FileName);
            FilePath = solution.FileName;
        }

        internal void UpdateProjects(EnvDTE.Solution solution)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var projects = DteProjectHelper.GetProjects(solution);

            var updated = new HashSet<string>();

            foreach (var project in projects)
            {
                try
                {
                    var guid = DteProjectHelper.GetProjectGuid(project);

                    if (!string.IsNullOrEmpty(guid))
                    {
                        updated.Add(guid);

                        if (!projectsMap.TryGetValue(guid, out var ideProject))
                        {
                            ideProject = new IdeProject(workspaceService);
                        }

                        log?.Info("Discovered project: " + project.Name);
                        ideProject.Update(this, project);

                        if (ideProject.IsLoaded)
                        {
                            projectsMap[guid] = ideProject;
                        }
                        else
                        {
                            if (projectsMap.ContainsKey(guid))
                            {
                                projectsMap.Remove(guid);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Possibly unloaded project visible in the DTE..
                }
            }

            var toRemove = projectsMap.Select(kp => kp.Key).Except(updated).ToList();

            foreach (var keys in toRemove)
            {
                projectsMap.Remove(keys);
            }
        }

        internal void AddProject(EnvDTE.Project project)
        {
            var guid = DteProjectHelper.GetProjectGuid(project);

            if (projectsMap.ContainsKey(guid))
            {
                log?.Info($"Trying to add the project {guid} to {Name} however it already exists in the shadow solution model. Use the update methods to update the information on the project.");
                return;
            }

            var ideProject = new IdeProject(workspaceService);
            ideProject.Update(this, project);

            projectsMap[guid] = ideProject;
        }

        internal void RemoveProject(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return;
            }

            if (projectsMap.ContainsKey(projectGuid))
            {
                projectsMap.Remove(projectGuid);
            }
        }

        internal void RenameProject(string projectGuid, Project project)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                throw new ArgumentException("message", nameof(projectGuid));
            }

            if (projectsMap.TryGetValue(projectGuid, out var ideProject))
            {
                ideProject.UpdateProperties(project);
            }
        }

        public IdeProject GetProjectByGuid(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                throw new ArgumentException("message", nameof(projectGuid));
            }

            if (projectsMap.TryGetValue(projectGuid, out var project))
            {
                return project;
            }

            return default;
        }

        internal IdeProject GetProjectByName(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentException("message", nameof(projectName));
            }

            foreach (var project in Projects)
            {
                if (project.Name == projectName)
                {
                    return project;
                }
            }

            return default;
        }
    }
}
