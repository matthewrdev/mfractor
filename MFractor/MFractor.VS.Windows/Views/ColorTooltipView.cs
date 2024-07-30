using System;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using MFractor.Analytics;
using MFractor.IOC;
using MFractor.Navigation;
using MFractor.Views.Branding;
using MFractor.VS.Windows.Utilities;
using MFractor.Work;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Brushes = System.Windows.Media.Brushes;

namespace MFractor.VS.Windows.Views
{
    public class ColorTooltipView : StackPanel
    {

        readonly Logging.ILogger log = Logging.Logger.Create();

        Label summaryLabel;
        Label colorLabel;

        public System.Windows.Media.Color Color { get; private set; }
        public string Label { get; private set; }

        public event EventHandler ColorClicked;

        public ColorTooltipView(System.Windows.Media.Color color,
                               INavigationSuggestion navigationSuggestion,
                               INavigationContext navigationContext,
                                string label = "")
        {
            Initialise(navigationSuggestion, navigationContext);
            SetColor(color, label);
        }

        void Initialise(INavigationSuggestion navigationSuggestion,
                               INavigationContext navigationContext)
        {
            try
            {
                this.Children.Clear();

                var textColor = TextColorHelper.GetThemeTextColor();
              
                summaryLabel = new Label();
                summaryLabel.Foreground = new SolidColorBrush(textColor);
                Children.Add(summaryLabel);

                colorLabel = new Label();
                Children.Add(colorLabel);


                if (navigationSuggestion != null && navigationContext != null)
                {
                    var fixLabel = new ClickableLabel(navigationSuggestion.Label, async () =>
                    {
                        var result = await Resolver.Resolve<INavigationService>().Navigate(navigationContext, navigationSuggestion);

                        if (result != null && result.Any())
                        {
                            Resolver.Resolve<IAnalyticsService>().Track(navigationSuggestion.Label);
                            await Resolver.Resolve<IWorkEngine>().ApplyAsync(result);
                        }
                    });

                    Children.Add(fixLabel);
                }

                Children.Add(new Separator());

                var brandedLabel = new Label()
                {
                    Content = BrandingHelper.BrandingText,
                    Foreground = new SolidColorBrush(textColor)
                };

                Children.Add(brandedLabel);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void SetColor(System.Windows.Media.Color color,
                             string label = "")
        {
            try
            {
                Color = color;
                Label = label;

                if (!string.IsNullOrEmpty(label))
                {
                    summaryLabel.Content = label;
                }
                else
                {
                    summaryLabel.Visibility = System.Windows.Visibility.Hidden;
                }

                colorLabel.Content = GetHexString(color);
                colorLabel.Width = 250;
                colorLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                colorLabel.Foreground = PerceivedBrightness(color) > 130 ? Brushes.Black : Brushes.White;
                colorLabel.Background = new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        // Credit: https://stackoverflow.com/a/2241471/1099111
        private int PerceivedBrightness(System.Windows.Media.Color c)
        {
            return (int)Math.Sqrt(
            (double)c.R * (double)c.R * .241 +
            (double)c.G * (double)c.G * .691 +
            (double)c.B * (double)c.B * .068);
        }

        public string GetHexString(System.Windows.Media.Color color)
        {
            return color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public string GetRGBAString(System.Windows.Media.Color color)
        {
            return color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString();
        }
    }
}
