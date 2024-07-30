using System;
using System.ComponentModel.Composition;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Views.Licensing;
using Xwt;

namespace MFractor.Views.Onboarding.Panels.Activation
{
    public class ImportLicenseControl : VBox
    {
        [Import]
        IMutableLicensingService LicensingService { get; set; }

        readonly LicenseWidget licenseWidget;
        readonly LinkLabel activateMFractoLite;

        public ImportLicenseControl(IActivationPanel activationPanel)
        {
            Spacing = 10;
            Resolver.ComposeParts(this);

            licenseWidget = new LicenseWidget(new LicenseWidgetOptions(true, false, false));

            PackStart(licenseWidget);

            PackStart(new HSeparator());

            activateMFractoLite = new LinkLabel("I'd like to use MFractor Lite")
            {
                HorizontalPlacement = WidgetPlacement.Center
            };
            activateMFractoLite.NavigateToUrl += ActivateMFractorPro_LinkClicked;
            ActivationPanel = activationPanel;

            PackStart(activateMFractoLite);

            LicensingService.OnLicenseActivated += LicensingService_OnLicenseActivated;
        }

        void LicensingService_OnLicenseActivated(object sender, LicenseActivatedEventArgs e)
        {
            ActivationPanel.NotifyActivationSuccess();
        }

        public IActivationPanel ActivationPanel { get; }

        void ActivateMFractorPro_LinkClicked(object sender, NavigateToUrlEventArgs e)
        {
            e.SetHandled();

            ActivationPanel.GoToActivateLite();
        }

        public void SetWindowFrame(WindowFrame windowFrame)
        {
            licenseWidget.SetWindowFrame(windowFrame);
        }
    }
}
