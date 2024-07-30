using System;
using System.IO;
using MFractor.Images;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.AppIconImporter
{
    public class AppIconImagePreview : VBox
    {
        readonly IImageUtilities imageSizeUtils;

        const int cPreviewBoxWidth = 200;
        const int cPreviewBoxHeight = 200;

        ImageView sizeErrorImage;
        ImageView sizeWarningImage;
        ImageView imagePreviewView;
        FrameBox imagePreviewFrame;
        Label imageSizeLabel;
        ImageSize? imageSize;

        public AppIconImagePreview(IImageUtilities imageSizeUtils)
        {
            this.imageSizeUtils = imageSizeUtils;
            Build();
        }

        void Build()
        {

            sizeErrorImage = new ImageView
            {
                Image = Image.FromResource("exclamation.png").WithSize(4.5, 15.5),
                TooltipText = "The image must have the same width and height.",
                WidthRequest = 15,
                Visible = false,
            };
            sizeWarningImage = new ImageView
            {
                Image = Image.FromResource("exclamation-yellow.png").WithSize(4.5, 15.5),
                TooltipText = "The recommended image size is 1024x1024 or larger.",
                WidthRequest = 15,
                Visible = false,
            };

            imagePreviewView = new ImageView
            {
                Image = GetPreviewPlaceholderImage(),
            };
            imagePreviewFrame = new FrameBox
            {
                WidthRequest = cPreviewBoxWidth,
                HeightRequest = cPreviewBoxHeight,
                VerticalPlacement = WidgetPlacement.Center,
                HorizontalPlacement = WidgetPlacement.Center,
                Content = imagePreviewView,
                BorderColor = new Color(.2, .2, .2),
                BorderWidth = 1,
            };
            imageSizeLabel = new Label
            {
                Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Normal),
                TextAlignment = Alignment.Center,
                Text = GetImageSizeDescription(null),
            };

            PackStart(imagePreviewFrame);

            var sizeBox = new HBox();
            sizeBox.PackStart(new HBox(), true, true);
            sizeBox.PackStart(sizeErrorImage);
            sizeBox.PackStart(sizeWarningImage);
            sizeBox.PackStart(imageSizeLabel);
            sizeBox.PackStart(new HBox(), true, true);
            PackStart(sizeBox);
        }

        public void SetImage(string filePath)
        {
            if (File.Exists(filePath))
            {
                imagePreviewView.Image = Image.FromFile(filePath).WithBoxSize(cPreviewBoxWidth, cPreviewBoxHeight);
                imageSize = GetImageSize(filePath);
            }
            else
            {
                imagePreviewView.Image = GetPreviewPlaceholderImage();
                imageSize = null;
            }

            UpdateSizeDescriptor(imageSize);
        }

        ImageSize GetImageSize(string filePath) => imageSizeUtils.GetImageSize(filePath);

        string GetImageSizeDescription(ImageSize? size)
        {
            var width = size?.Width.ToString() ?? "N/A";
            var height = size?.Height.ToString() ?? "N/A";
            return $"Width: {width} | Height: {height}";
        }

        Image GetPreviewPlaceholderImage() => Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(cPreviewBoxWidth, cPreviewBoxHeight);

        void UpdateSizeDescriptor(ImageSize size)
        {
            imageSizeLabel.Text = GetImageSizeDescription(size);
            sizeErrorImage.Visible = IsSizeError(size);
            sizeWarningImage.Visible = !sizeErrorImage.Visible && IsSizeLowerThanRecommended(size);
        }

        bool IsSizeError(ImageSize size) => size?.Width != size?.Height;

        bool IsSizeLowerThanRecommended(ImageSize size) => size?.Width < 1024;
    }
}
