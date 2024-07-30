using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;

namespace MFractor.Ide.Commands.SolutionPad
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class EditSharedProjectItemsCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Edit Shared Projects Items";


        [ImportingConstructor]
        public EditSharedProjectItemsCommand(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        ISharedAssetsProject GetTargetProject(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is ISharedAssetsProject project)
            {
                return project;
            }

            return null;
        }

        public void Execute(ICommandContext commandContext)
        {
            var sharedAssetsProject = GetTargetProject(commandContext);

            WorkEngine.ApplyAsync(new OpenFileWorkUnit(sharedAssetsProject.ProjectItemsFilePath, default));
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var sharedAssetsProject = GetTargetProject(commandContext);

            if (sharedAssetsProject == null
                || !File.Exists(sharedAssetsProject.ProjectItemsFilePath))
            {
                return null;
            }

            return new CommandState()
            {
                Label = "Edit Shared Project Items",
                Description = "Opens the .projitems file in the XML editor.",
                CanExecute = true,
                IsVisible = true,
            };
        }
    }
}
