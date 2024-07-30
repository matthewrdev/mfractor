using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Fonts.WorkUnits;
using MFractor.Ide.Commands;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ImportFontCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Import Font Asset";

        [ImportingConstructor]
        public ImportFontCommand(Lazy<IWorkspaceService> workspaceService, Lazy<IWorkEngine> workEngine)
        {
            this.workspaceService = workspaceService;
            this.workEngine = workEngine;
        }

        Solution GetSolution(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                return GetSolution(solutionPadCommandContext);
            }

            var workspace = WorkspaceService.CurrentWorkspace;

            if (workspace != null && workspace.CurrentSolution != null)
            {
                return workspace.CurrentSolution;
            }

            return null;
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
                if (folder.IsAndroidImageFolder()
                    || folder.IsAndroidAssetsFolder()
                    || folder.IsAppleUnifiedImageFolder())
                {
                    return folder.Project.Solution;
                }
            }

            return default;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var solution = GetSolution(commandContext);

            var isAvailable = solution != null && solution.GetMobileProjects().Any();

            return new CommandState(isAvailable, isAvailable, "Font", "Import a new font asset");
        }

        public void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new ImportFontWorkUnit()
            {
                Solution = GetSolution(commandContext),
            });
        }
    }
}
