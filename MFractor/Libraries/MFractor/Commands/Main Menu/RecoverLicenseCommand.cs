using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Licensing.Recovery;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class RecoverLicenseCommand : ICommand
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<ILicenseRecoveryService> licenseRecoveryService;
        public ILicenseRecoveryService LicenseRecoveryService => licenseRecoveryService.Value;

        [ImportingConstructor]
        public RecoverLicenseCommand(Lazy<IWorkEngine> workEngine,
                                     Lazy<IAnalyticsService> analyticsService,
                                     Lazy<ILicenseRecoveryService> licenseRecoveryService,
                                     Lazy<IDialogsService> dialogsService)
        {
            this.workEngine = workEngine;
            this.analyticsService = analyticsService;
            this.licenseRecoveryService = licenseRecoveryService;
            this.dialogsService = dialogsService;
        }

        public void Execute(ICommandContext commandContext)
        {
            IReadOnlyList<IWorkUnit> onLicenseRecoveryRequested(string email)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(1000);

                    var validation = new EmailValidationHelper();

                    AnalyticsService.Track("License Recover Request");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    LicenseRecoveryService.RecoverLicense(email).ConfigureAwait(false); // Intentionally let this run in the background.
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    DialogsService.Confirm($"A request to recover your MFractor license for {email} has been raised.\n\nIf this email address is associated with a valid license, you will recieve your license soon.", "Ok");
                });

                return Array.Empty<IWorkUnit>();
            }

            bool validateEmailInput(string email, out string message)
            {
                message = string.Empty;
                var validation = new EmailValidationHelper();

                if (validation.IsValidEmail(email))
                {
                    return true;
                }

                message = $"{email} is not a valid email address";
                return false; ;
            }

            WorkEngine.ApplyAsync(new TextInputWorkUnit("Recover MFractor Professional License",
                                                                "Please enter the email address that was used to purchase your MFractor Professional license",
                                                                string.Empty,
                                                                "Recover License",
                                                                "Cancel",
                                                                onLicenseRecoveryRequested,
                                                                validateEmailInput));
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Recover MFractor Professional License",
                Description = "Recover your MFractor Professional License via email"
            };
        }
    }
}
