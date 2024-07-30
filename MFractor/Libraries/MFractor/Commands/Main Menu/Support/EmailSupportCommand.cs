using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace MFractor.Commands.MainMenu.Support
{
    [Export]
    class EmailSupportCommand : ICommand
    {
        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public EmailSupportCommand(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }
        public void Execute(ICommandContext commandContext)
        {
            UrlLauncher.OpenUrl("mailto:matthew@mfractor.com");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Email",
                Description = "Email matthew@mfractor.com for support"
            };
        }
    }
}
