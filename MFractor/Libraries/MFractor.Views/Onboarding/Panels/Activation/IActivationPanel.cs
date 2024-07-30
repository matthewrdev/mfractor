using System;
namespace MFractor.Views.Onboarding.Panels.Activation
{
    public interface IActivationPanel : IOnboardingPanel
    {
        void GoToChooseActivationMethod();

        void GoToActivateLite();

        void GoToActivatePro();

        void NotifyActivationSuccess();
    }
}
