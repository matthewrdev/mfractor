using System;
using System.ComponentModel.Composition;
using MFractor.Licensing;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ViewLicenseCommand : ICommand
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        [ImportingConstructor]
        public ViewLicenseCommand(Lazy<IWorkEngine> workEngine,
                                  Lazy<ILicensingService> licensingService)
        {
            this.workEngine = workEngine;
            this.licensingService = licensingService;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var label = LicensingService.IsPaid ? "Edit License" : "Activate License";

            return new CommandState(label, "View and edit MFractors licensing information.");
        }

        public void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(new LicenseDialogWorkUnit()).ConfigureAwait(false);
        }
    }
}
