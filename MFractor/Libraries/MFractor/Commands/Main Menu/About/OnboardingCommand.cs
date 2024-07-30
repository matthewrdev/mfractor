using System;
using System.ComponentModel.Composition;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Commands.MainMenu.About
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class OnboardingCommand : ICommand
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public OnboardingCommand(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        public void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new OnboardingDialogWorkUnit()).ConfigureAwait(false);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Onboarding",
                Description = "Launch the onboarding dialog",
            };
        }
    }
}
