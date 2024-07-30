using System.Windows.Controls;
using MFractor.IOC;
using MFractor.Views.Branding;
using MFractor.VS.Windows.Utilities;

namespace MFractor.VS.Windows.Views
{
    class BrandedFooter : StackPanel
    {
        public BrandedFooter(string helpUrl = "")
        {
            Orientation = Orientation.Horizontal;

            var image = new Image()
            {
                Source = BitmapImageHelper.GetSourceForOnRender("logo-16.png"),
            };
            image.Width = 8;
            image.Height = 8;

            Children.Add(image);
            Children.Add(new Label()
            {
                Content = BrandingHelper.BrandingText,
                Foreground = TextColorHelper.GetThemeTextBrush(),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            });

            if (!string.IsNullOrEmpty(helpUrl))
            {
                Children.Add(new ClickableLabel("Help", async () =>
                {
                    Resolver.Resolve<IUrlLauncher>().OpenUrl(helpUrl);
                })
                {
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                });
            }
        }
    }
}
