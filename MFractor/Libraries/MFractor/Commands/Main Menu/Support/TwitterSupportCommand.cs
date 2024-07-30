using System;
using System.ComponentModel.Composition;

namespace MFractor.Commands.MainMenu.Support
{
    [Export]
    class TwitterSupportCommand : ICommand
    {
        readonly Lazy<IUrlLauncher> urlLauncher;        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public TwitterSupportCommand(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }

        public void Execute(ICommandContext commandContext)
        {
            UrlLauncher.OpenUrl("https://twitter.com/matthewrdev");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Twitter",
                Description = "Open Twitter for help and support"
            };
        }
    }
}

