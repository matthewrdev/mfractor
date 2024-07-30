using System;
using System.ComponentModel.Composition;
using MFractor;

namespace MFractor.Commands.MainMenu.Support
{
    [Export]
    class DocumentationSupportCommand : ICommand
    {
        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public DocumentationSupportCommand(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }

        public void Execute(ICommandContext commandContext)
        {
            UrlLauncher.OpenUrl("https://docs.mfractor.com");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Documentation",
                Description = "Visit MFractors documentation"
            };
        }
    }
}
