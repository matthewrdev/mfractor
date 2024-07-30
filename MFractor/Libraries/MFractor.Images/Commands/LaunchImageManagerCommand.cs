using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Ide.Commands;
using MFractor.Images.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILaunchImageManagerCommand))]
    class LaunchImageManagerCommand : ILaunchImageManagerCommand, IToolCommand, IAnalyticsFeature
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Manage Image Assets";

        [ImportingConstructor]
        public LaunchImageManagerCommand(Lazy<IWorkspaceService> workspaceService,
                                        Lazy<IWorkEngine> workEngine)
        {
            this.workspaceService = workspaceService;
            this.workEngine = workEngine;
        }

        public void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new OpenImageManagerWorkUnit()
            {
                Solution = WorkspaceService.CurrentWorkspace.CurrentSolution,
            }).ConfigureAwait(false);
        }

        Solution GetSolution(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                var solution = default(Solution);

                var item = solutionPadCommandContext.SelectedItem;

                if (item is Solution s)
                {
                    solution = s;
                }
                else if (item is Project project)
                {
                    if (project.IsMobileProject())
                    {
                        solution = project.Solution;
                    }
                }
                else if (item is IProjectFolder folder)
                {
                    if (folder.IsAndroidImageFolder() || folder.IsAppleUnifiedImageFolder())
                    {
                        solution = folder.Project.Solution;
                    }
                }

                return solution;
            }

            return WorkspaceService.CurrentWorkspace?.CurrentSolution;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
           var solution = GetSolution(commandContext);

            var available = solution != null && !string.IsNullOrEmpty(solution.FilePath);

            return new CommandState(available, available, "Manage Image Assets", "Launches MFractors Image Manager, a useful tool to visually explore the image assets for your Android and iOS projects.");
        }
    }
}
