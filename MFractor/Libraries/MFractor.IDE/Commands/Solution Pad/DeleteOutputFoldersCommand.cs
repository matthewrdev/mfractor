using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Ide.WorkUnits;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Commands.SolutionPad
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class DeleteOutputFoldersCommand  : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Delete Output Folders";

        [ImportingConstructor]
        public DeleteOutputFoldersCommand(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        public void Execute(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;

            if (solutionPadContext.SelectedItem is Solution solution)
            {
                WorkEngine.ApplyAsync(new DeleteOutputFoldersWorkUnit()
                {
                    Solution = solution
                });
            }
            else if (solutionPadContext.SelectedItem is Project project)
            {
                WorkEngine.ApplyAsync(new DeleteOutputFoldersWorkUnit()
                {
                    Project = project
                });
            }
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var solutionPadContext = commandContext as ISolutionPadCommandContext;

            if (solutionPadContext == null)
            {
                return default;
            }

            if (solutionPadContext.SelectedItem is Solution
                || solutionPadContext.SelectedItem is Project)
            {
                return new CommandState()
                {
                    Label = "Delete Output Folders",
                    Description = "Delete the output folders for the given solution or project.",
                    CanExecute = true,
                    IsVisible = true,
                };
            }

            return default;
        }
    }
}
