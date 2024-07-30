using System;
using System.Collections.Generic;
using System.Linq;
using Xwt;

namespace MFractor.Views.Onboarding.Footer
{
    public class OnboardingProgressFooter : HBox
    {
        public IEnumerable<IOnboardingPanel> Panels { get; }

        List<OnboardingFooterElement> elements = new List<OnboardingFooterElement>();

        public OnboardingProgressFooter(IEnumerable<IOnboardingPanel> panels)
        {
            Panels = panels;

            foreach (var panel in panels)
            {
                var element = new OnboardingFooterElement(panel)
                {
                    HorizontalPlacement = WidgetPlacement.Center,
                };
                var container = new FrameBox()
                {
                    BorderWidth = 0,
                };
                container.Content = element;
                PackStart(container, true, true);

                if (panels.Last() != panel)
                {
                    PackStart(new VSeparator());
                }

                elements.Add(element);
            }
        }

        public void SetCurrentPanel(IOnboardingPanel panel)
        {
            foreach (var element in elements)
            {
                element.IsFocused = element.Panel == panel;
            }
        }
    }
}
