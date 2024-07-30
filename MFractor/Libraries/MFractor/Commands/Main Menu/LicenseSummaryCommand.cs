using System;
using System.ComponentModel.Composition;
using MFractor.Licensing;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class LicenseSummaryCommand : ICommand
    {
        readonly Lazy<ILicensingService> licensingService;
        ILicensingService LicensingService => licensingService.Value;

        [ImportingConstructor]
        public LicenseSummaryCommand(Lazy<ILicensingService> licensingService)
        {
            this.licensingService = licensingService;
        }

        public void Execute(ICommandContext commandContext)
        {
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var text = "MFractor Is Not Activated";
            var description = string.Empty;

            if (LicensingService.HasActivation)
            {
                var details = LicensingService.LicensingDetails;
                description = "Activated by: " + LicensingService.ActivationEmail;
                if (details.IsTrial)
                {
                    description += "\nLicensed to:" + details.Name + " (" + details.Email + ")";
                    text = "Trial License (" + (details.Expiry.Value - DateTime.UtcNow).Days + " days remaining)";
                }
                else if (details.IsPaid)
                {
                    description += "\nLicensed to:" + details.Name + " (" + details.Email + ")";
                    text = "Professional License (" + (details.Expiry.Value - DateTime.UtcNow).Days + " days remaining)";
                }
            }

            return new CommandState(true, false, text, description);
        }
    }
}
