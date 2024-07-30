using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code;
using MFractor.Ide.WorkUnitHandlers;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace.WorkUnits;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Projects.SharedAssetsProjects;
using MFractor.Ide;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class IdeCreateProjectFileWorkUnitHandler : BaseCreateProjectFileWorkUnitHandler
    {
        [ImportingConstructor]
        public IdeCreateProjectFileWorkUnitHandler(Lazy<IFileCreationPostProcessorRepository> fileCreationPostProcessors,
                                                   Lazy<IDialogsService> dialogsService)
            : base(fileCreationPostProcessors, dialogsService)
        {
        }

        static Project ResolveIdeProject(CreateProjectFileWorkUnit workUnit, Project project)
        {
            var solution = project.ParentSolution;
            var projects = solution.GetAllProjects();

            if (workUnit.InferWhenInSharedProject
                && projects.Any(p => p is SharedAssetsProject))
            {
                if (!(project is SharedAssetsProject))
                {
                    // Check if the active document maps to a shared project.
                    var activeDoc = IdeApp.Workbench.ActiveDocument;

                    if (activeDoc?.Owner is MonoDevelop.Projects.DotNetProject docProject
                        && docProject.References != null)
                    {
                        foreach (var r in docProject.References)
                        {
                            if (r.ReferenceType == MonoDevelop.Projects.ReferenceType.Project)
                            {
                                if (r.ResolveProject(solution) is SharedAssetsProject sharedProject)
                                {
                                    var isInShared = sharedProject.Items.OfType<MonoDevelop.Projects.ProjectFile>().Any(i => i.FilePath == activeDoc.Name);
                                    if (isInShared)
                                    {
                                        project = sharedProject;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return project;
        }

        protected override void AddMSBuildEntry(string diskPath, Microsoft.CodeAnalysis.Project project, CreateProjectFileWorkUnit workUnit)
        {
            Xwt.Application.Invoke(async () =>
            {
                var ideProject = ResolveIdeProject(workUnit, project.ToIdeProject());
                var existingFile = ideProject.GetProjectFile(diskPath);
                var isNetstandard = IsNetstandardProject(ideProject);

                if (existingFile != null)
                {
                    return;
                }

                var file = workUnit.HasBuildAction ? ideProject.AddFile(diskPath, workUnit.BuildAction) : ideProject.AddFile(diskPath);
                file.Visible = workUnit.Visible;

                if (!string.IsNullOrEmpty(workUnit.DependsUponFile))
                {
                    file.DependsOn = workUnit.DependsUponFile;
                }
                if (!string.IsNullOrEmpty(workUnit.ResourceId))
                {
                    file.ResourceId = workUnit.ResourceId;
                }
                if (!string.IsNullOrEmpty(workUnit.CustomToolNamespace))
                {
                    file.CustomToolNamespace = workUnit.CustomToolNamespace;
                }
                if (!string.IsNullOrEmpty(workUnit.Generator))
                {
                    file.Generator = workUnit.Generator;
                }
                if (!string.IsNullOrEmpty(workUnit.BuildAction))
                {
                    file.BuildAction = workUnit.BuildAction;
                }

                using (var monitor = new ProgressMonitor())
                {
                    await ideProject.SaveAsync(monitor);
                }
            });
        }

        protected override async Task AddVirtualDirectory(Microsoft.CodeAnalysis.Project project, string virtualDirectoryPath)
        {
            await Xwt.Application.InvokeAsync(() =>
            {
                var ideProject = project.ToIdeProject();
                ideProject.AddDirectory(virtualDirectoryPath);
            });
        }

        static bool IsNetstandardProject(Project project)
        {
            if (project is DotNetProject dotNetProject)
            {
                return dotNetProject.TargetFramework.Id.Identifier.Equals(".netstandard", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
