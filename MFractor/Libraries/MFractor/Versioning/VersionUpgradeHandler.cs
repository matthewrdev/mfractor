using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Attributes;
using MFractor.Configuration;
using MFractor.Licensing;

namespace MFractor.LifecycleHandlers
{
    [ApplicationLifecyclePriority(uint.MinValue)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class VersionUpgradeHandler : IApplicationLifecycleHandler
    {
        readonly Lazy<IUserOptions> userOptions;
        IUserOptions UserOptions => userOptions.Value;

        readonly Lazy<IProductInformation> productInformation;
        IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<ILicensingService> licensingService;
        ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        IAnalyticsService AnalyticsService => analyticsService.Value;

        [ImportingConstructor]
        public VersionUpgradeHandler(Lazy<IUserOptions> userOptions,
                                     Lazy<IProductInformation> productInformation,
                                     Lazy<ILicensingService> licensingService,
                                     Lazy<IAnalyticsService> analyticsService)
        {
            this.userOptions = userOptions;
            this.productInformation = productInformation;
            this.licensingService = licensingService;
            this.analyticsService = analyticsService;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            TrackVersionUpgrades();
        }

        void TrackVersionUpgrades()
        {
            const string LastVersionKey = "mfractor.keys.last_run_version";
            var currentVersion = ProductInformation.Version.ToShortString();

            if (UserOptions.HasKey(LastVersionKey))
            {
                var lastVersion = UserOptions.Get(LastVersionKey, default(string));

                if (!string.IsNullOrEmpty(lastVersion) && lastVersion != currentVersion)
                {
                    var traits = new Dictionary<string, string>()
                    {
                        { "user", LicensingService.ActivationEmail }
                    };

                    AnalyticsService.Track($"Version Upgrade ({lastVersion} to {currentVersion})", traits);
                }
            }
            else
            {
                AnalyticsService.Track("Installation");
            }

            UserOptions.Set(LastVersionKey, currentVersion);
        }
    }
}
