using System;

namespace MFractor.Views.Onboarding
{
    public interface IOnboardingPanel
    {
        Xwt.Widget Widget { get; }

        string Title { get; }

        bool IsComplete { get; set; }

        event EventHandler OnCompletedChanged;

        event EventHandler OnNext;

        event EventHandler OnPrevious;

        void Activated();

        void Deactivated();

        void SetWindowFrame(Xwt.WindowFrame windowFrame);
    }
}
