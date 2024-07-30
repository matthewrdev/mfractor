using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Ide.Commands;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ImportIconWizardCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        readonly Lazy<IWorkEngine> workEngine;

        IWorkspaceService WorkspaceService => workspaceService.Value;

        IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Import Icon Wizard";

        [ImportingConstructor]
        public ImportIconWizardCommand(
            Lazy<IWorkspaceService> workspaceService,
            Lazy<IWorkEngine> workEngine)
        {
            this.workspaceService = workspaceService;
            this.workEngine = workEngine;
        }

        IReadOnlyList<Project> GetAvailableProjects(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                var solution = GetSolution(solutionPadCommandContext);

                if (solution == null)
                {
                    return Array.Empty<Project>();
                }

                return solution.GetMobileProjects();
            }

            var workspace = WorkspaceService.CurrentWorkspace;

            if (workspace != null && workspace.CurrentSolution != null)
            {
                var solution = workspace.CurrentSolution;

                return solution.GetMobileProjects();
            }

            return Array.Empty<Project>();
        }

        Solution GetSolution(ISolutionPadCommandContext solutionPadCommandContext)
        {
            var item = solutionPadCommandContext.SelectedItem;

            if (item is Solution solution)
            {
                return solution;
            }
            else if (item is Project project)
            {
                if (project.IsMobileProject())
                {
                    return project.Solution;
                }
            }
            else if (item is IProjectFolder folder)
            {
                if (folder.IsAndroidImageFolder() || folder.IsAndroidAssetsFolder() || folder.IsAppleUnifiedImageFolder())
                {
                    return folder.Project.Solution;
                }
            }

            return default;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var projects = GetAvailableProjects(commandContext).Where(p => !p.IsMauiProject()).ToList();
            return new CommandState(projects.Any(), projects.Any(), "App Icon", "Import an Application Icon into your Android or iOS projects using the Image Importer.");
        }

        public void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new ImportIconWizardWorkUnit()
            {
                Projects = GetAvailableProjects(commandContext).Where(p => !p.IsMauiProject()).ToList(),
            });
        }
    }
}
