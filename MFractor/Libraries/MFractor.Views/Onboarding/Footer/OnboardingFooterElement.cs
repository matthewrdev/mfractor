using System;
using Xwt;

namespace MFractor.Views.Onboarding.Footer
{
    class OnboardingFooterElement : Xwt.HBox
    {
        Label label;
        ImageView imageView;

        public string Text
        {
            get => label.Text;
            set => label.Text = value;
        }

        bool isComplete;

        public IOnboardingPanel Panel { get; }

        public bool IsComplete
        {
            get => isComplete;
            set
            {
                isComplete = value;
                imageView.Visible = isComplete;
            }
        }

        bool isFocused;
        public bool IsFocused
        {
            get => isFocused;
            set
            {
                isFocused = value;
                label.Font = isFocused ? Xwt.Drawing.Font.SystemFont.WithWeight(Xwt.Drawing.FontWeight.Bold).WithScaledSize(1.15) : Xwt.Drawing.Font.SystemFont.WithWeight(Xwt.Drawing.FontWeight.Light);
            }
        }

        public OnboardingFooterElement(IOnboardingPanel panel)
        {
            label = new Label();

            imageView = new ImageView(Xwt.Drawing.Image.FromResource("check-green.png").WithSize(18, 18))
            {
                HorizontalPlacement = WidgetPlacement.Center,
                VerticalPlacement = WidgetPlacement.Center,
            };

            Panel = panel;

            Text = panel.Title;
            IsComplete = panel.IsComplete;
            panel.OnCompletedChanged += OnboardingPanel_OnCompletedChanged;

            PackStart(imageView);
            PackStart(label);
        }

        void OnboardingPanel_OnCompletedChanged(object sender, EventArgs e)
        {
            IsComplete = Panel.IsComplete;
        }
    }
}
