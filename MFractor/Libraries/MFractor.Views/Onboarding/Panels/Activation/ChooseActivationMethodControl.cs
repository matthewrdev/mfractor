using System.ComponentModel.Composition;
using MFractor.IOC;
using Xwt;

namespace MFractor.Views.Onboarding.Panels.Activation
{
    public class ChooseActivationMethodControl : VBox
    {
        [Import]
        public IUrlLauncher UrlLauncher { get; set; }

        readonly LinkLabel activateMFractorLite;
        readonly Label activateMFractorLiteDescription;
        readonly LinkLabel activateMFractorPro;
        readonly Label activateMFractorProDescription;

        public ChooseActivationMethodControl(IActivationPanel activationPanel)
        {
            Spacing = 10;
            Resolver.ComposeParts(this);

            VerticalPlacement = WidgetPlacement.Center;
            HorizontalPlacement = WidgetPlacement.Center;
            ExpandHorizontal = true;

            ActivationPanel = activationPanel;

            activateMFractorLite = new LinkLabel("I'd like to try MFractor")
            {
                HorizontalPlacement = WidgetPlacement.Center
            };
            activateMFractorLite.NavigateToUrl += ActivateMFractorLite_NavigateToUrl;
            activateMFractorLite.Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.6).WithWeight(Xwt.Drawing.FontWeight.Bold);
            activateMFractorLiteDescription = new Label()
            {
                Markup = "<b>MFractor Lite is our free usage tier and includes a 30 day trial of MFractor Professional.</b>\n\nPerfect for hobbyist developers or students, use MFractor Lite if you'd like to try MFractor before purchasing.",
                Wrap = WrapMode.Word,
                TextAlignment = Alignment.Center,
            };

            PackStart(activateMFractorLite);
            PackStart(activateMFractorLiteDescription);

            PackStart(new HSeparator());

            activateMFractorPro = new LinkLabel("I have an MFractor Professional license")
            {
                HorizontalPlacement = WidgetPlacement.Center
            };
            activateMFractorPro.NavigateToUrl += ActivateMFractorPro_LinkClicked;
            activateMFractorPro.Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.6).WithWeight(Xwt.Drawing.FontWeight.Bold);
            activateMFractorProDescription = new Label()
            {
                Markup = "<b>MFractor Professional is our paid tier.</b>\n\nMFractor Professional is for professional app developers seeking a productivity boost in their day to day work.",
                Wrap = WrapMode.Word,
                TextAlignment = Alignment.Center,
            };

            PackStart(activateMFractorPro);
            PackStart(activateMFractorProDescription);
        }

        void ActivateMFractorLite_NavigateToUrl(object sender, NavigateToUrlEventArgs e)
        {
            e.SetHandled();
            ActivationPanel.GoToActivateLite();
        }

        void ActivateMFractorPro_LinkClicked(object sender, NavigateToUrlEventArgs e)
        {
            e.SetHandled();
            ActivationPanel.GoToActivatePro();
        }

        public IActivationPanel ActivationPanel { get; }
    }
}
