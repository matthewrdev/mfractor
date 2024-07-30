using System;
using System.ComponentModel.Composition;
using MFractor.Licensing;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class VersionSummaryCommand : ICommand
    {
        readonly Lazy<ILicensingService> licensingService;
        ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        [ImportingConstructor]
        public VersionSummaryCommand(Lazy<ILicensingService> licensingService,
                                     Lazy<IProductInformation> productInformation)
        {
            this.licensingService = licensingService;
            this.productInformation = productInformation;
        }

        public void Execute(ICommandContext commandContext)
        {
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var text = "MFractor ";
            var description = string.Empty;

            if (LicensingService.HasActivation)
            {
                if (LicensingService.IsTrial)
                {
                    text += "(Trial)";
                }
                else if (LicensingService.IsPaid)
                {
                    text += "(Professional)";
                }
                else
                {
                    text += "(Lite)";
                }
            }
            else
            {
                text += "(Unactivated)";
            }

            text += " - v" + ProductInformation.Version.ToShortString();

            return new CommandState(true, false, text, description);
        }
    }
}
