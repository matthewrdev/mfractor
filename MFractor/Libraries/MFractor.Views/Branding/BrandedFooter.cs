using System;
using MFractor.Analytics;
using MFractor.IOC;
using Xwt;

namespace MFractor.Views.Branding
{
    public class BrandedFooter : HBox
    {
        readonly string featureName;
        readonly bool isExperimental;

        ImageView imageView;
        Label label;
        LinkLabel helpLabel;
        Spinner progressSpinner;

        string helpUrl;

        /// <summary>
        /// A flag indicating if the progress spinner should be presented.
        /// </summary>
        public bool IsInProgress
        {
            get => progressSpinner.Animate;
            set
            {
                progressSpinner.Animate = value;
                progressSpinner.Visible = value;
            }
        }

        public BrandedFooter(string helpUrl = "", string featureName = "", bool isExperimental = false)
        {
            Build();

            HelpUrl = helpUrl;

            this.featureName = featureName;
            this.isExperimental = isExperimental;
        }

        void HelpLabel_NavigateToUrl(object sender, NavigateToUrlEventArgs e)
        {
            e.SetHandled();

            var launcher = Resolver.Resolve<IUrlLauncher>();

            launcher.OpenUrl(HelpUrl);
        }

        public string HelpUrl
        {
            get => helpUrl;
            set
            {
                helpUrl = value;

                helpLabel.NavigateToUrl -= HelpLabel_NavigateToUrl;
                helpLabel.Visible = !string.IsNullOrEmpty(helpUrl);
                if (helpLabel.Visible)
                {
                    helpLabel.Uri = new Uri(helpUrl);
                    helpLabel.NavigateToUrl += HelpLabel_NavigateToUrl;
                }
            }
        }

        void Build()
        {
            imageView = new ImageView()
            {
                Image = Xwt.Drawing.Image.FromResource("mfractor_logo.png").WithSize(10, 10),
                VerticalPlacement = WidgetPlacement.Center
            };

            PackStart(imageView);

            label = new Label()
            {
                Text = BrandingHelper.BrandingText,
                Font = BrandingHelper.BrandingFontStyle,
            };

            if (isExperimental)
            {
                label.Text += " - (Experimental Feature)";
            }

            PackStart(label, true, true);

            progressSpinner = new Spinner
            {
                Animate = false,
                Visible = false,
            };
            PackStart(progressSpinner);

            helpLabel = new LinkLabel()
            {
                Text = "Help",
            };
            helpLabel.LinkClicked += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(featureName))
                {
                    Resolver.Resolve<IAnalyticsService>().Track($"Help Link Clicked ({featureName})");
                }
                else
                {
                    Resolver.Resolve<IAnalyticsService>().Track("Help Link Clicked");
                }
            };

            PackStart(helpLabel);
        }
    }
}
