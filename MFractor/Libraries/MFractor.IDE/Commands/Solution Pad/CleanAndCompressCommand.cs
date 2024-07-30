using System;
using System.ComponentModel.Composition;
using MFractor.Commands;

namespace MFractor.Ide.Commands.SolutionPad
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class CleanAndCompressCommand : ICommand
    {
        readonly Lazy<IDialogsService> dialogsService;
        IDialogsService DialogsService => dialogsService.Value;

        [ImportingConstructor]
        public CleanAndCompressCommand(Lazy<IDialogsService> dialogsService)
        {
            this.dialogsService = dialogsService;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return null;
        }

        public void Execute(ICommandContext commandContext)
        {
            // TODO: Requres implementation.
        }
    }
}
