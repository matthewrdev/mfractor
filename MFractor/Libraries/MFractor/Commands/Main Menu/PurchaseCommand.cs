using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Licensing;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class PurchaseCommand : ICommand
    {
        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public PurchaseCommand(Lazy<ILicensingService> licensingService,
                               Lazy<IAnalyticsService> analyticsService,
                               Lazy<IUrlLauncher> urlLauncher)
        {
            this.licensingService = licensingService;
            this.analyticsService = analyticsService;
            this.urlLauncher = urlLauncher;
        }

        public void Execute(ICommandContext commandContext)
        {
            AnalyticsService.Track("Buy MFractor Clicked");

            UrlLauncher.OpenUrl("https://www.mfractor.com/buy");
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var text = "Buy MFractor";

            return new CommandState(!LicensingService.IsPaid, true, text, "Go to www.mfractor.com/buy");
        }
    }
}
