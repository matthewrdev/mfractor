using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace MFractor.Commands.MainMenu.About
{
    [Export]
    class EndUserLicenseCommand : ICommand
    {
        readonly Lazy<IUrlLauncher> urlLauncher;        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public EndUserLicenseCommand(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }

        public void Execute(ICommandContext commandContext)
        {
            UrlLauncher.OpenUrl("https://docs.mfractor.com/legal/end-user-license.pdf");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "End User License",
                Description = "View MFractors end user license"
            };
        }
    }
}
