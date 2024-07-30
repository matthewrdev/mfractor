using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace MFractor.Commands.MainMenu.Support
{
    [Export]
    class GitterSupportCommand : ICommand
    {
        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public GitterSupportCommand(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }
        public void Execute(ICommandContext commandContext)
        {
            UrlLauncher.OpenUrl("https://gitter.im/mfractor/Lobby#");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Gitter",
                Description = "Open MFractors Gitter channel for help and support"
            };
        }
    }
}
