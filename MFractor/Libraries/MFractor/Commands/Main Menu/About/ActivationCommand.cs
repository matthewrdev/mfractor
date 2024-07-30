using System;
using System.ComponentModel.Composition;
using MFractor.Commands;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ActivationCommand : WorkUnitCommand
    {
        [ImportingConstructor]
        public ActivationCommand(Lazy<IWorkEngine> workEngine)
            : base("Activate", "Edit the name and email that you used to activate MFractor.", new ActivationDialogWorkUnit(), workEngine)
        {
        }
    }
}
