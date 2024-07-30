using System;
using MFractor.Views.Activation;

namespace MFractor.Views.RequestTrial
{
    class RequestTrialDialog : Xwt.Dialog
    {
        readonly ActivationControl activationControl;

        public RequestTrialDialog()
        {
            Width = 400;

            Title = "Request MFractor 30-Day Trial";
            Icon = Xwt.Drawing.Image.FromResource("mfractor_logo.png");

            activationControl = new ActivationControl(false, false, "Request Trial");

            Content = activationControl;
            activationControl.OnSuccessfulActivation += ActivationControl_OnSuccessfulActivation;
        }

        void ActivationControl_OnSuccessfulActivation(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }
}
