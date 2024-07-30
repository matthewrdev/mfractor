using System;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.ImageImporter.Preview
{
    public class OutputImageSummary : Xwt.VBox, IDisposable
    {
        Label nameLabel;
        FrameBox frame;
        ImageView image;
        Label sizeLabel;

        const int baseWidth = 100;
        const int baseHeight= 100;

        public OutputImageSummary()
        {
            Build();
        }

        void Build()
        {
            nameLabel = new Label()
            {
                TextAlignment = Alignment.Center,
                Font = Font.SystemFont.WithSize(10).WithWeight(FontWeight.Bold),
            };

            this.frame = new FrameBox()
            {
                HeightRequest = 110,
                WidthRequest = 110,
                VerticalPlacement = WidgetPlacement.Center,
                HorizontalPlacement = WidgetPlacement.Center,
                BorderColor = new Color(.2, .2, .2),
                BorderWidth = 1,
            };

            image = new ImageView()
            {
                HorizontalPlacement = WidgetPlacement.Center,
                VerticalPlacement = WidgetPlacement.Center,
            };

            frame.Content = image;

            sizeLabel = new Label()
            {
                TextAlignment = Alignment.Center,
                Font = Font.SystemFont.WithSize(10).WithWeight(FontWeight.Bold),
            };

            PackStart(frame);
            PackStart(sizeLabel);
            PackStart(nameLabel);
        }

        public void Update(string filePath, ImageSize size, Image image, double scale)
        {
            Update(filePath, size);
            Update(image, scale);
        }

        public void Update(string filePath, ImageSize size)
        {
            nameLabel.Text = filePath.Length > 50 ? (filePath.Substring(0, 50 - 3) + "...") : filePath;
            nameLabel.TooltipText = filePath;
            sizeLabel.Text = size.Width + "w by " + size.Height + "h";
        }

        public void Update(Image image, double scale)
        {
            this.image.Image = image.WithBoxSize(baseWidth * scale, baseHeight * scale);
        }

        protected override void Dispose(bool disposing)
        {
            this.image.Dispose();

            base.Dispose(disposing);
        }
    }
}
