using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace MFractor.Commands.MainMenu.About
{
    [Export]
    class PrivacyPolicyCommand : ICommand
    {
        readonly Lazy<IUrlLauncher> urlLauncher;        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public PrivacyPolicyCommand(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }

        public void Execute(ICommandContext commandContext)
        {
            UrlLauncher.OpenUrl("https://docs.mfractor.com/legal/privacy-policy.pdf");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Privacy Policy",
                Description = "View MFractors Privacy Policy"
            };
        }
    }
}
