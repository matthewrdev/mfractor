using System;

namespace MFractor.Views.Onboarding
{
    public class OnboardingDialog : Xwt.Dialog
    {
        readonly OnboardingControl control;

        public OnboardingDialog()
        {
            Title = "Welcome To MFractor";
            Width = 1024;
            Height = 720;
            InitialLocation = Xwt.WindowLocation.CenterScreen;

            control = new OnboardingControl();
            control.OnCompleted += Control_OnCompleted;

            this.Content = control;
            control.SetWindowFrame(this);
        }

        void Control_OnCompleted(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
