using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ViewReleaseNotesCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IReleaseNotesService> releaseNotesService;
        public IReleaseNotesService ReleaseNotesService => releaseNotesService.Value;

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        public string AnalyticsEvent => "View Release Notes";

        [ImportingConstructor]
        public ViewReleaseNotesCommand(Lazy<IReleaseNotesService> releaseNotesService,
                                       Lazy<IUrlLauncher> urlLauncher,
                                       Lazy<IProductInformation> productInformation)
        {
            this.releaseNotesService = releaseNotesService;
            this.urlLauncher = urlLauncher;
            this.productInformation = productInformation;
        }

        public void Execute(ICommandContext commandContext)
        {
            var version = ProductInformation.Version;
            var url = ReleaseNotesService.GetReleaseNotesUrl(version);

            UrlLauncher.OpenUrl(url);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Release Notes",
                Description = "Open the release notes for the current version of MFractor",
            };
        }
    }
}
