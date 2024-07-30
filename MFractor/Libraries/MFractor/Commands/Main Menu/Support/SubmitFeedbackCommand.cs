using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Net;
using MFractor.Analytics;
using MFractor;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class SubmitFeedbackCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<IEnvironmentDetailsService> environmentDetailsService;
        public IEnvironmentDetailsService EnvironmentDetailsService => environmentDetailsService.Value;

        readonly Lazy<IClipboard> clipboard;
        public IClipboard Clipboard => clipboard.Value;

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public SubmitFeedbackCommand(Lazy<IEnvironmentDetailsService> environmentDetailsService,
                                     Lazy<IClipboard> clipboard,
                                     Lazy<IUrlLauncher> urlLauncher)
        {
            this.environmentDetailsService = environmentDetailsService;
            this.clipboard = clipboard;
            this.urlLauncher = urlLauncher;
        }

        public string AnalyticsEvent => "Submit Feedback";

        public void Execute(ICommandContext commandContext)
        {
            var buildInfo = string.Join("\n", EnvironmentDetailsService.Details);

            Clipboard.Text = buildInfo;

            var body = WebUtility.UrlEncode("\n\n##Installation Information\n\n```\n" + buildInfo + "\n```");

            var url = feedbackUrl + "?body=" + body;

            UrlLauncher.OpenUrl(url, false);
        }

        const string feedbackUrl = "https://github.com/mfractor/mfractor-feedback/issues/new";

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Submit Feedback",
                Description = "Create a github issue and submit product feedback."
            };
        }
    }
}
