using System;
using MFractor.Utilities;
using Xwt;

namespace MFractor.Views.Onboarding.Panels
{
    public class LicenseAgreementPanel : VBox, IOnboardingPanel
    {
        Label licenseContent;
        ScrollView licenseScrollView;

        public LicenseAgreementPanel()
        {
            PackStart(new Label("Please read and agree to our End User License")
            {
                    Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.5).WithWeight(Xwt.Drawing.FontWeight.Bold)
            });

            PackStart(new HSeparator());

            licenseContent = new Label
            {
                Wrap = WrapMode.Word,
                Text = ResourcesHelper.ReadResourceContent(this, "end-user-license.txt")
            };

            licenseScrollView = new ScrollView()
            {
                Content = licenseContent,
                ExpandHorizontal = true,
                HeightRequest = 420,
            };

            PackStart(licenseScrollView);

            PackStart(new HSeparator());

            acknowledgement = new CheckBox("I Accept");
            acknowledgement.Toggled += Acknowledgement_Toggled;

            PackStart(acknowledgement);

            PackStart(new HSeparator());

            // Title

            // License contents

            continueButton = new Button("Continue");
            continueButton.Clicked += ContinueButton_Clicked;
            continueButton.Sensitive = false;


            PackStart(continueButton);
            // Checkbox acknowledging license

            // Button to continue when check box is ticked.
        }

        void Acknowledgement_Toggled(object sender, EventArgs e)
        {
            continueButton.Sensitive = acknowledgement.Active;
        }

        void ContinueButton_Clicked(object sender, EventArgs e)
        {
            IsComplete = true;
            OnNext?.Invoke(this, EventArgs.Empty);
        }

        public Widget Widget => this;

        public string Title => "License Agreement";

        bool isComplete;

        CheckBox acknowledgement;
        Button continueButton;

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

        public void Activated()
        {
        }

        public void Deactivated()
        {
        }

        public void SetWindowFrame(WindowFrame windowFrame)
        {
        }
    }
}
