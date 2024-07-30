using System;
using MFractor.Views.Activation;
using Xwt;

namespace MFractor.Views.Onboarding.Panels.Activation
{
    public class ActivateLiteControl : VBox
    {
        readonly ActivationControl activationControl;
        readonly LinkLabel activateMFractorPro;

        public ActivateLiteControl(IActivationPanel activationPanel)
        {
            activationControl = new ActivationControl(false, false);
            activationControl.OnSuccessfulActivation += ActivationControl_OnSuccessfulActivation;

            PackStart(activationControl, true, true);
            PackStart(new HSeparator());

            activateMFractorPro = new LinkLabel("I'd like to import a Professional License")
            {
                HorizontalPlacement = WidgetPlacement.Center
            };
            activateMFractorPro.NavigateToUrl += ActivateMFractorPro_LinkClicked;
            ActivationPanel = activationPanel;

            // Link to change to import "Actually, I'd like to import a Professional License"

            PackStart(activateMFractorPro);
        }

        void ActivationControl_OnSuccessfulActivation(object sender, EventArgs e)
        {
            ActivationPanel.NotifyActivationSuccess();
        }

        public IActivationPanel ActivationPanel { get; }

        void ActivateMFractorPro_LinkClicked(object sender, NavigateToUrlEventArgs e)
        {
            e.SetHandled();

            ActivationPanel.GoToActivatePro();
        }
    }
}
