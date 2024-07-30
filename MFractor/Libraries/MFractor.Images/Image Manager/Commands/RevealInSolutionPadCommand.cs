using System;
using System.ComponentModel.Composition;
using MFractor.Commands;
using MFractor.Ide.WorkUnits;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Images.ImageManager.Commands
{
    class RevealInSolutionPadCommand : ImageManagerCommand
    {
        public override string AnalyticsEvent => "Reveal In Solution Pad Command";

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public RevealInSolutionPadCommand(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new SelectFileInProjectPadWorkUnit(commandContext.ProjectFile));
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (commandContext.ProjectFile == null)
            {
                return default;
            }

            return new CommandState(true, true, $"Reveal In Solution Pad", "Locates and selects the image asset in the solution pad");
        }
    }
}
