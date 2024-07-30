using System;
using Xwt;

namespace MFractor.Views.Onboarding.Panels
{
    public class WelcomePanel : VBox, IOnboardingPanel
    {
        public WelcomePanel()
        {
            Build();
        }

        void Build()
        {
            WidthRequest = 600;
            Spacing = 20;
            VerticalPlacement = WidgetPlacement.Center;
            HorizontalPlacement = WidgetPlacement.Center;

            image = new ImageView(Xwt.Drawing.Image.FromResource("mfractor_logo.png").WithSize(250, 247))
            {
                HorizontalPlacement = WidgetPlacement.Center
            };

            PackStart(image);

            welcomeText = new Label()
            {
                Markup = "Welcome to MFractor, a powerful productivity tool for Xamarin Developers.\n\nClick 'Continue' to get started.",
                TextAlignment = Alignment.Center,
                Wrap = WrapMode.Word,
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.4),
            };

            PackStart(welcomeText);

            continueButton = new Button("Continue");

            continueButton.Clicked += ContinueButton_Clicked;

            PackStart(continueButton);

            PackStart(new Label()
            {
                Markup = "You may close this dialog at any time and re-open it by going to <b>MFractor</b> then <b>About</b> then <b>Onboarding</b>",
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(0.8),
                TextAlignment = Alignment.Center,
            });

        }

        void ContinueButton_Clicked(object sender, EventArgs e)
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

        public Widget Widget => this;

        public string Title => "Welcome";

        bool isComplete = false;
        ImageView image;
        Label welcomeText;
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

        public void SetWindowFrame(WindowFrame windowFrame)
        {
        }
    }
}
