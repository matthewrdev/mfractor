using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace MFractor.Commands.MainMenu.Support
{
    [Export]
    class SlackSupportCommand : ICommand
    {
        readonly IUrlLauncher urlLauncher;

        [ImportingConstructor]
        public SlackSupportCommand(IUrlLauncher urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }
        public void Execute(ICommandContext commandContext)
        {
            urlLauncher.OpenUrl("https://xamarinchat.slack.com/archives/mfractor", false);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Slack",
                Description = "Visit MFractors Slack channel"
            };
        }
    }
}
