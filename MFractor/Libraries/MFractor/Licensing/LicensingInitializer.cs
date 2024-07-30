using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Attributes;

using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Licensing
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ApplicationLifecyclePriority(uint.MinValue)]
    class LicensingInitializer : IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<ILicensingService> licensingService;
        ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IDialogsService> dialogsService;
        IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IProductInformation> productInformation;
        IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IReleaseNotesService> releaseNotesService;
        IReleaseNotesService ReleaseNotesService => releaseNotesService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public LicensingInitializer(Lazy<ILicensingService> licensingService,
                                    Lazy<IAnalyticsService> analyticsService,
                                    Lazy<IDialogsService> dialogsService,
                                    Lazy<IProductInformation> productInformation,
                                    Lazy<IReleaseNotesService> releaseNotesService,
                                    Lazy<IWorkEngine> workEngine,
                                    Lazy<IUrlLauncher> urlLauncher)
        {
            this.licensingService = licensingService;
            this.analyticsService = analyticsService;
            this.dialogsService = dialogsService;
            this.productInformation = productInformation;
            this.releaseNotesService = releaseNotesService;
            this.workEngine = workEngine;
            this.urlLauncher = urlLauncher;
        }

        public void Startup()
        {
            PerformLicenseStatusCheck();

            ProcessPendingActivation();
        }

        public void Shutdown()
        {
        }

        void PerformLicenseStatusCheck()
        {
            var licenseStatus = LicensingService.GetLicenseStatusMessage();

            if (licenseStatus != null)
            {
                AnalyticsService.Track(licenseStatus.LicenseStatusMessageKind);
                var result = DialogsService.Confirm(licenseStatus.Title + "\n" + licenseStatus.Message, "Ok");

                if (result)
                {
                    UrlLauncher.OpenUrl("https://www.mfractor.com/buy");
                }
            }
        }

        public void ProcessPendingActivation()
        {
            var version = ProductInformation.Version.ToShortString();
            if (LicensingService.HasActivation == false)
            {
                WorkEngine.ApplyAsync(new OnboardingDialogWorkUnit()).ConfigureAwait(false);

                ReleaseNotesService.SetLatestVersion(version);
                return;
            }

            var status = ReleaseNotesService.GetVersionUpdateStatus(version);

            if (status != VersionUpdateStatus.None)
            {
                log?.Info("Upgrade to " + version);
                if (status == VersionUpdateStatus.SignificantVersionUpgrade)
                {
                    if (!string.IsNullOrEmpty(ProductInformation.VersionMarketingUrl))
                    {
                        UrlLauncher.OpenUrl($"{ProductInformation.VersionMarketingUrl}{$"?utm_medium=version_upgrade"}");
                    }
                }
                else
                {

                    void viewReleaseNotes()
                    {
                        var releaseNotesUrl = ReleaseNotesService.GetReleaseNotesUrl(ProductInformation.Version);

                        UrlLauncher.OpenUrl(releaseNotesUrl);
                        AnalyticsService.Track($"View Release Notes ({ProductInformation.Version})");
                    }

                    DialogsService.ToolbarMessage($"Thanks for updating MFractor! You are now on version {version}", new ToolBarAction("Release Notes", viewReleaseNotes));
                }
            }
        }
    }
}
