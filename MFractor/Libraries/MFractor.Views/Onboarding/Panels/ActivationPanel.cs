using System;
using MFractor.Views.Onboarding.Panels.Activation;
using Xwt;

namespace MFractor.Views.Onboarding.Panels
{
    public class ActivationPanel : VBox, IActivationPanel, IOnboardingPanel
    {
        public ActivationPanel()
        {
            VerticalPlacement = WidgetPlacement.Center;
            HorizontalPlacement = WidgetPlacement.Center;

            chooseMethodControl = new Activation.ChooseActivationMethodControl(this)
            {
                VerticalPlacement = WidgetPlacement.Center,
            };
            activateLiteControl = new Activation.ActivateLiteControl(this)
            {
                VerticalPlacement = WidgetPlacement.Center,
            };
            importLicenseControl = new Activation.ImportLicenseControl(this)
            {
                VerticalPlacement = WidgetPlacement.Center,
            };

            frame = new FrameBox()
            {
                WidthRequest = 600,
                BorderWidth = 0,
            };

            PackStart(frame, true, true);

            GoToChooseActivationMethod();
        }

        public Widget Widget => this;

        public string Title => "Activation";

        readonly ChooseActivationMethodControl chooseMethodControl;
        readonly ActivateLiteControl activateLiteControl;
        readonly ImportLicenseControl importLicenseControl;
        readonly FrameBox frame;

        bool isComplete;

        public bool IsComplete
        {
            get => isComplete;
            set
            {
                isComplete = value;
                OnCompletedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler OnNext;

        public event EventHandler OnPrevious;

        public event EventHandler OnCompletedChanged;

        public void GoToChooseActivationMethod()
        {
            if (frame.Content != chooseMethodControl)
            {
                frame.Content = chooseMethodControl;
            }
        }

        public void GoToActivateLite()
        {
            if (frame.Content != activateLiteControl)
            {
                frame.Content = activateLiteControl;
            }
        }

        public void GoToActivatePro()
        {
            if (frame.Content != importLicenseControl)
            {
                frame.Content = importLicenseControl;
            }
        }

        public void NotifyActivationSuccess()
        {
            IsComplete = true;
            OnNext?.Invoke(this, EventArgs.Empty);
        }

        public void Activated()
        {
        }

        public void Deactivated()
        {
        }

        public void SetWindowFrame(WindowFrame windowFrame)
        {
            importLicenseControl.SetWindowFrame(windowFrame);
        }
    }
}
