using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;
using MFractor.VS.Mac.Utilities;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Projects.SharedAssetsProjects;
using MFractor.Work;
using MFractor.Ide.WorkUnits;
using MFractor.Workspace;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class SelectFileInProjectPadWorkUnitHandler : WorkUnitHandler<SelectFileInProjectPadWorkUnit>
    {
        public override Task<IWorkExecutionResult> OnExecute(SelectFileInProjectPadWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            SelectFile(workUnit.ProjectIdentifier.ToIdeProject(), workUnit.FilePath, workUnit.InferWhenInSharedProject);

            return Task.FromResult(default(IWorkExecutionResult));
        }

        public const string PadId = "ProjectPad";
        public const string ProjectSolutionPadClass = "MonoDevelop.Ide.Gui.Pads.ProjectPad.ProjectSolutionPad";

        public static readonly Type ProjectSolutionPadType = typeof(Pad).Assembly.GetType(ProjectSolutionPadClass);

        public const string SelectFileMethodName = "SelectFile";
        public static readonly MethodInfo SelectFileMethod = ProjectSolutionPadType.GetMethod(SelectFileMethodName, BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SelectFile(IProjectFile projectFile, bool inferWhenInSharedProject = true)
        {
            if (projectFile == null)
            {
                return;
            }

            var ideProject = projectFile.CompilationProject.ToIdeProject();

            SelectFile(ideProject, projectFile.FilePath, inferWhenInSharedProject);
        }

        public static void SelectFile(Project project, string file, bool inferWhenInSharedProject = true)
        {
            var pad = GetProjectSolutionPad();

            if (pad == null)
            {
                return;
            }

            var solution = project.ParentSolution;
            var projects = solution.GetAllProjects();

            if (inferWhenInSharedProject
                && projects.Any(p => p is SharedAssetsProject))
            {
                if (!(project is SharedAssetsProject))
                {
                    // Check if the active document maps to a shared project.
                    var activeDoc = IdeApp.Workbench.ActiveDocument;
                    if (activeDoc != null)
                    {
                        if (activeDoc.Owner is MonoDevelop.Projects.DotNetProject docProject)
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
                        else if (activeDoc.Owner is SharedAssetsProject sharedProject && project is DotNetProject dnp)
                        {
                            foreach (var r in dnp.References)
                            {
                                if (r.ReferenceType == MonoDevelop.Projects.ReferenceType.Project)
                                {
                                    if (r.ResolveProject(solution) is SharedAssetsProject sp && sp.ItemId == sharedProject.ItemId)
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
            }

            try
            {
                SelectFileMethod.Invoke(pad.Content, new object[] { project, file });
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

        }

        public static Pad GetProjectSolutionPad()
        {
            foreach (var pad in IdeApp.Workbench.Pads)
            {
                if (string.Equals(pad.Id, PadId, StringComparison.OrdinalIgnoreCase))
                {
                    return pad;
                }
            }

            return default;
        }
    }
}
