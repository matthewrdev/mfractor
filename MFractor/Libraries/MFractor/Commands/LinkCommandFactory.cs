using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;

namespace MFractor.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILinkCommandFactory))]
    class LinkCommandFactory : ILinkCommandFactory
    {
        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public LinkCommandFactory(Lazy<IProductInformation> productInformation,
                                  Lazy<IAnalyticsService> analyticsService,
                                  Lazy<IUrlLauncher> urlLauncher)
        {
            this.productInformation = productInformation;
            this.analyticsService = analyticsService;
            this.urlLauncher = urlLauncher;
        }

        public ILinkCommand Create(string label, string url)
        {
            var command = new LinkCommand(label, url, UrlLauncher);
            command.LinkClicked += Command_LinkClicked;

            return command;
        }

        void Command_LinkClicked(object sender, LinkCommandClickedEventArgs e)
        {
            AnalyticsService.Track($"Link Clicked ({e.LinkCommand.Label})");
        }

        public ILinkCommand Create(string label, string url, string descripion)
        {
            var command = new LinkCommand(label, descripion, UrlLauncher);
            command.LinkClicked += Command_LinkClicked;

            return command;

        }
    }
}