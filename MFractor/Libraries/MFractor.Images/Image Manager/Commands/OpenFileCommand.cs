using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using MFractor.Commands;
using MFractor.Work;

namespace MFractor.Images.ImageManager.Commands
{
    class OpenFileCommand : ImageManagerCommand
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        public override string AnalyticsEvent => "Open Image File Command";

        [ImportingConstructor]
        public OpenFileCommand(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            Process.Start(commandContext.ProjectFile.FilePath);
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (commandContext.ProjectFile == null)
            {
                return default;
            }

            return new CommandState(true, true, $"Open", "Opens the image asset in the default external image viewer");
        }
    }
}
