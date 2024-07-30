using System;
using System.ComponentModel.Composition;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ThirdPartyAttributionCommand : ICommand
    {
        readonly Lazy<IUrlLauncher> urlLauncher;        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public ThirdPartyAttributionCommand(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }

        public void Execute(ICommandContext commandContext)
        {
            UrlLauncher.OpenUrl("https://docs.mfractor.com/legal/third-party-software");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Third Party Software Attribution",
                Description = "Attribution to the third party and open source software that MFractor uses."
            };
        }
    }
}
