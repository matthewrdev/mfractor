using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Documentation;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ResyncSolutionResources : IToolCommand, IAnalyticsFeature, IAmDocumented
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public ResyncSolutionResources(Lazy<IWorkspaceService> workspaceService,
                                       Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.workspaceService = workspaceService;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public string AnalyticsEvent => Name;

        public string Name => "Resync Solution Resources";

        public string Documentation => "Triggers a resync of MFractors solution resources database.";

        public void Execute(ICommandContext commandContext)
        {
            var solution = GetTargetSolution(commandContext);

            ResourcesDatabaseEngine.SynchroniseSolutionResources(solution);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var solution = GetTargetSolution(commandContext);

            if (solution == null)
            {
                return null;
            }

            var available = solution != null && !string.IsNullOrEmpty(solution.FilePath);

            return new CommandState(available, available, "Resync MFractor Solution Resources", "Triggers MFractor to resynchronise the resources data of the currently active solution in the workspace.");
        }

        Solution GetTargetSolution(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                return solutionPadCommandContext.SelectedItem as Solution;
            }

            return WorkspaceService.CurrentWorkspace?.CurrentSolution;
        }
    }
}
