using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MFractor.Views.Branding;
using Microsoft.VisualStudio.Debugger.Interop;

namespace MFractor.VS.Windows.Views
{
    /// <summary>
    /// An MFractor branded image tooltip.
    /// </summary>
    public class ImageFileTooltipView : StackPanel
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        Image imageView;

        public string ImageAsset { get; private set; }
        public System.Drawing.Size ImageSize { get; private set; }

        public event EventHandler Clicked;

        public ImageFileTooltipView(string imageAsset, 
                                    System.Drawing.Size imageSize)
        {
            Orientation = Orientation.Vertical;
            Initialise();
            SetImage(imageAsset, imageSize);
        }

        void Initialise()
        {
            try
            {
                imageView = new Image();

                Children.Add(imageView);

                Children.Add(new Separator());

                Children.Add(new BrandedFooter());
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void SetImage(string imageFilePath, System.Drawing.Size imageSize)
        {
            try
            {
                ImageAsset = imageFilePath;
                ImageSize = imageSize;

                if (imageFilePath != null && imageSize.Height > 0)
                {
                    var uri = new Uri(imageFilePath);
                    var source = new BitmapImage(uri);
                    imageView.Source = source;
                    imageView.Width = imageSize.Width;
                    imageView.Height = imageSize.Height;
                }
                else
                {
                    imageView.Source = null;
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
