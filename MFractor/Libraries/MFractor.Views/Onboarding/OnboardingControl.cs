using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Views.Onboarding.Footer;
using MFractor.Views.Onboarding.Panels;
using Xwt;

namespace MFractor.Views.Onboarding
{
    public class OnboardingControl : VBox
    {
        Label titleLabel;
        List<IOnboardingPanel> panels;

        OnboardingProgressFooter footer;

        Frame panelFrame;

        public OnboardingControl()
        {
            Build();

            SetPanel<WelcomePanel>();
        }

        public event EventHandler OnCompleted;

        public void SetWindowFrame(WindowFrame windowFrame)
        {
            foreach (var panel in panels)
            {
                panel.SetWindowFrame(windowFrame);
            }
        }

        void Build()
        {
            titleLabel = new Label()
            {
                Font = Xwt.Drawing.Font.SystemFont.WithWeight(Xwt.Drawing.FontWeight.Bold).WithScaledSize(2),
                TextAlignment = Alignment.Center,
            };
            PackStart(titleLabel);

            PackStart(new HSeparator());

            panels = new List<IOnboardingPanel>()
            {
                new WelcomePanel(),
                new LicenseAgreementPanel(),
                new ActivationPanel(),
                new GettingStartedPanel(),
            };

            panelFrame = new Frame();

            PackStart(panelFrame, true, true);

            footer = new OnboardingProgressFooter(panels);

            PackStart(footer);

            foreach (var panel in panels)
            {
                panel.OnNext += Panel_OnNext;
                panel.OnPrevious += Panel_OnPrevious;
            }

            SetPanel<WelcomePanel>();
        }

        void Panel_OnPrevious(object sender, EventArgs e)
        {
            if (panels.Contains(sender))
            {
                var index = panels.IndexOf(sender);

                index--;

                if (index >= 0)
                {
                    SetPanel(panels[index]);
                }
            }
        }

        void Panel_OnNext(object sender, EventArgs e)
        {
            if (panels.Contains(sender))
            {
                var index = panels.IndexOf(sender);

                index++;

                if (index < panels.Count)
                {
                    SetPanel(panels[index]);
                }
                else
                {
                    OnCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        void SetPanel<TPanel>() where TPanel : IOnboardingPanel
        {
            var panel = panels.OfType<TPanel>().FirstOrDefault();

            SetPanel(panel);
        }

        void SetPanel(IOnboardingPanel panel)
        {
            if (panel != null)
            {
                foreach (var p in panels)
                {
                    if (p ==  panel)
                    {
                        p.Activated();
                    }
                    else
                    {
                        p.Deactivated();
                    }
                }
                panelFrame.Content = panel.Widget;
                titleLabel.Text = panel.Title;

                footer.SetCurrentPanel(panel);
            }
        }
    }
}
