using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Documentation;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;

namespace MFractor.Ide.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [RequiresLicense]
    [Export]
    class CopyResourceIDToClipboard : ICommand, IAnalyticsFeature, IAmDocumented
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => Name;

        public string Name => "Copy ResourceID to Clipboard";

        public string Documentation => "The Copy ResourceID to Clipboard shortcut let's you right click on a EmbeddedResource file in the project pad and copy it's resource ID to the clipboard.";

        [ImportingConstructor]
        public CopyResourceIDToClipboard(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        IProjectFile GetTargetProjectFile(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is IProjectFile projectFile)
            {
                return projectFile;
            }

            return null;
        }

        public void Execute(ICommandContext commandContext)
        {
            var projectFile = GetTargetProjectFile(commandContext);

            WorkEngine.ApplyAsync(new CopyValueToClipboardWorkUnit()
            {
                Value = projectFile.ResourceId,
                Message = "Copied " + projectFile.ResourceId + " to clipboard.",
            }).ConfigureAwait(false);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var projectFile = GetTargetProjectFile(commandContext);

            if (projectFile == null
                || string.IsNullOrEmpty(projectFile.BuildAction)
                || string.IsNullOrEmpty(projectFile.ResourceId)
                || projectFile.BuildAction.Equals("EmbeddedResource", StringComparison.OrdinalIgnoreCase) == false)
            {
                return null;
            }

            return new CommandState()
            {
                Label = "Copy resource ID to clipboard",
                Description = "Copy the ID of this embedded resource to the clipboard",
                CanExecute = true,
                IsVisible = true,
            };
        }
    }
}
