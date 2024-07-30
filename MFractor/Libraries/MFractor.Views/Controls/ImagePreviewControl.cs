using System.IO;
using MFractor.IOC;
using MFractor.Utilities;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Controls
{
    public class ImagePreviewControl : VBox
    {
        ImageView imagePreview;
        FrameBox imageFrame;
        Label fileSizeLabel;
        Label sizeLabel;

        public ImagePreviewControl()
        {
            Build();
        }

        public void SetImage(string imageFilePath)
        {
            if (string.IsNullOrEmpty(imageFilePath) || !File.Exists(imageFilePath) || !ImageHelper.IsImageFile(imageFilePath))
            {
                sizeLabel.Text = "Width: NA | Height: NA";
                fileSizeLabel.Text = "File Size: NA";
                imagePreview.Image = Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(250, 220);
            }
            else
            {
                var imageUtil = Resolver.Resolve<IImageUtilities>();
                var size = imageUtil.GetImageSize(imageFilePath);

                sizeLabel.Text = $"Width: {size.Width} | Height: {size.Height}";
                fileSizeLabel.Text = "File Size: " + FileSizeHelper.GetFormattedFileSize(imageFilePath);
                imagePreview.Image = Image.FromFile(imageFilePath).WithBoxSize(250, 300);
            }
        }

        void Build()
        {
            imagePreview = new ImageView
            {
                Image = Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(250, 220)
            };

            imageFrame = new FrameBox()
            {
                HeightRequest = 320,
                WidthRequest = 280,
                VerticalPlacement = WidgetPlacement.Center,
                HorizontalPlacement = WidgetPlacement.Center,
                Content = imagePreview,
                BorderColor = new Color(.2, .2, .2),
                BorderWidth = 1,
            };

            PackStart(imageFrame, true, true);

            fileSizeLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = "File Size: NA",
            };

            sizeLabel = new Label()
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = "Width: NA | Height: NA",
            };

            PackStart(fileSizeLabel);
            PackStart(sizeLabel);
        }
    }
}
