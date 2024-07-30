using System;
using MFractor.Views.Controls;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.AppIconImporter
{
    public class IconItemPreview : VBox
    {
        public const int PreviewSize = 48;

        FrameBox imageFrame;
        ImageView iconImageView;
        Label nameLabel;

        public string ImageName
        {
            get => nameLabel.Text;
            set => nameLabel.Text = value;
        }

        public IconItemPreview()
        {
            Build();
        }

        void Build()
        {
            imageFrame = new FrameBox
            {
                WidthRequest = PreviewSize,
                HeightRequest = PreviewSize,
                VerticalPlacement = WidgetPlacement.Center,
                HorizontalPlacement = WidgetPlacement.Center,
                BorderColor = new Color(.2, .2, .2),
                BorderWidth = 1,
            };
            iconImageView = new ImageView
            {
                HorizontalPlacement = WidgetPlacement.Center,
                VerticalPlacement = WidgetPlacement.Center,
                Image = Image.FromResource("mfractor_logo_grayscale.png").WithBoxSize(PreviewSize - 1),
            };
            nameLabel = new BoldCenteredLabel();

            imageFrame.Content = iconImageView;
            PackStart(imageFrame);
            PackStart(nameLabel);
        }

        public void SetImage(Image image) => iconImageView.Image = image;

    }
}
