using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using MFractor.Workspace;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows.WorkspaceModel
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IWorkspaceShadowModel))]
    [Export(typeof(IMutableWorkspaceShadowModel))]
    class MutableWorkspaceShadowModel : IWorkspaceShadowModel, IMutableWorkspaceShadowModel
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public MutableWorkspaceShadowModel(Lazy<IWorkspaceService> workspaceService)
        {
            this.workspaceService = workspaceService;
        }

        readonly Lazy<IWorkspaceService> workspaceService;
        IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Dictionary<string, IdeSolution> solutionMap = new Dictionary<string, IdeSolution>();
        public IReadOnlyList<IdeSolution> Solutions => solutionMap.Values.ToList();

        public bool HasSolution(string solutionName)
        {
            if (string.IsNullOrEmpty(solutionName))
            {
                return false;
            }

            return solutionMap.ContainsKey(solutionName);
        }

        public void AddSolution(EnvDTE.Solution solution)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(AddSolution) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var solutionName = Path.GetFileName(solution.FileName);
            if (solutionMap.ContainsKey(solutionName))
            {
                log?.Info("A solution entry for " + solutionName + " already exist. If trying to update the solutions shadow copy, use the update methods directly.");
                return;
            }

            log?.Info("Building shadow model for solution: " + solutionName);

            var ideSolution = new IdeSolution(WorkspaceService);

            try
            {
                ideSolution.Update(solution);
                solutionMap[solutionName] = ideSolution;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void RemoveSolution(string solutionName)
        {
            if (solutionMap.ContainsKey(solutionName))
            {
                solutionMap.Remove(solutionName);
            }
        }

        public void AddProjectToSolution(string solutionName, EnvDTE.Project project)
        {
            if (!ThreadHelper.CheckAccess())
            {
                log?.Warning(nameof(AddProjectToSolution) + " called from non ui thread");
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            if (solutionName is null)
            {
                throw new ArgumentNullException(nameof(solutionName));
            }

            if (solutionMap.TryGetValue(solutionName, out var ideSolution))
            {
                ideSolution.AddProject(project);
            }
            else
            {
                log?.Warning($"Tried to add a {project.Name} to {solutionName} however the solution is not tracked by the shadow workspace.");
            }
        }

        public void RemoveProjectFromSolution(string solutionName, string projectGuid)
        {
            if (string.IsNullOrEmpty(solutionName))
            {
                throw new ArgumentException("message", nameof(solutionName));
            }

            if (solutionMap.TryGetValue(solutionName, out var ideSolution))
            {
                ideSolution.RemoveProject(projectGuid);
            }
            else
            {
                log?.Warning($"Tried to remove the project {projectGuid} from {solutionName} however the solution is not tracked by the shadow workspace.");
            }
        }

        public void RenameProject(string solutionName, string projectGuid, Project project)
        {
            if (solutionMap.TryGetValue(solutionName, out var ideSolution))
            {
                ideSolution.RenameProject(projectGuid, project);
            }
            else
            {
                log?.Warning($"Tried to rename the project {projectGuid} however the solution is not tracked by the shadow workspace.");
            }
        }

        public void AddFileToProject(ProjectItem projectItem, string projectGuid)
        {
            var ideProject = GetProjectByGuid(projectGuid);

            if (ideProject == null)
            {
                log?.Warning($"Failed to add the project file as the proejct {projectGuid} is not tracked by the shadow workspace.");
                return;
            }

            ideProject.AddProjectFile(projectItem);
        }

        public void RemoveFileFromProject(string filePath, string projectGuid)
        {
            var ideProject = GetProjectByGuid(projectGuid);

            if (ideProject == null)
            {
                log?.Warning($"Failed to remove the project file as the proejct {projectGuid} is not tracked by the shadow workspace.");
                return;
            }

            ideProject.RemoveProjectFile(filePath);
        }

        public void RenameFile(string oldFilePath, string newFilePath, string projectGuid)
        {
            var ideProject = GetProjectByGuid(projectGuid);

            if (ideProject == null)
            {
                log?.Warning($"Failed to remove the project file as the proejct {projectGuid} is not tracked by the shadow workspace.");
                return;
            }

            ideProject.RenameFile(oldFilePath, newFilePath);
        }

        public IdeProject GetProjectByGuid(string projectGuid)
        {
            if (string.IsNullOrEmpty(projectGuid))
            {
                return default;
            }

            foreach (var solution in Solutions)
            {
                var project = solution.GetProjectByGuid(projectGuid);

                if (project != null)
                {
                    return project;
                }
            }

            return default;
        }

        public IdeProject GetProjectByName(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                return default;
            }

            foreach (var solution in Solutions)
            {
                var project = solution.GetProjectByName(projectName);

                if (project != null)
                {
                    return project;
                }
            }

            return default;
        }

        public void RenameSolution(EnvDTE.Solution solution, string oldName)
        {
            if (solutionMap.ContainsKey(oldName))
            {
                var ideSolution = solutionMap[oldName];
                solutionMap.Remove(oldName);
                ideSolution.Update(solution);
            }
        }

        public IProjectFile GetProjectFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return default;
            }

            if (!Solutions.Any())
            {
                return default;
            }

            foreach (var solution in Solutions)
            {
                foreach (var project in solution.Projects)
                {
                    var file = project.GetProjectFile(filePath);
                    if (file != null)
                    {
                        return file;
                    }
                }
            }

            return default;
        }
    }
}
