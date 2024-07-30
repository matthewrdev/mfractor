using System;
using System.ComponentModel.Composition;
using MFractor.VS.Mac.Utilities;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Projects;
using MonoDevelop.Projects.SharedAssetsProjects;
using System.Linq;
using MFractor.Workspace;
using MFractor.Ide;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ISolutionPad))]
    class IdeSolutionPad : ISolutionPad
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        [ImportingConstructor]
        public IdeSolutionPad(Lazy<IWorkspaceService> workspaceService)
        {
            this.workspaceService = workspaceService;
        }

        public object SelectedItem
        {
            get
            {
                var selectedItem = IdeApp.ProjectOperations.CurrentSelectedItem;

                if (selectedItem is Solution solution)
                {
                    return WorkspaceService.Workspaces.FirstOrDefault(w => w.CurrentSolution.FilePath == solution.FileName)?.CurrentSolution;
                }
                else if (selectedItem is Project project && !(selectedItem is SharedAssetsProject))
                {
                    return project.ToCompilationProject();
                }
                else if (selectedItem is SharedAssetsProject sharedAssetsProject)
                {
                    return new IdeSharedAssetsProject(sharedAssetsProject);
                }
                else if (selectedItem is ProjectFolder folder)
                {
                    return new IdeProjectFolder(folder);
                }
                else if (selectedItem is ProjectFile projectFile)
                {
                    return new IdeProjectFile(projectFile, projectFile.Project.ToCompilationProject());
                }
                else if (selectedItem is ProjectReference projectReference)
                {
                    switch (projectReference.ReferenceType)
                    {
                        case ReferenceType.Assembly:
                            break;
                        case ReferenceType.Project:
                            break;
                        case ReferenceType.Package:
                            break;
                        case ReferenceType.Custom:
                            break;
                    }
                }

                return null;
            }
        }
    }
}