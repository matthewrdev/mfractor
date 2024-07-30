using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MFractor.Images.Models;
using MFractor.Views.Controls;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.AppIconImporter
{
    public class IconSetPreview : VBox
    {
        const int itemSpacing = 8;
        const int horizontalMargin = 8;

        readonly List<IconItemPreview> iconPreviews = new List<IconItemPreview>();
        readonly IEnumerable<IconImage> images;

        Label setNameLabel;
        Label detailsLabel;
        Label sizeLabel;

        public IconSetPreview(IEnumerable<IconImage> images)
        {
            this.images = images;
            Build();
        }

        void Build()
        {
            setNameLabel = new CenteredLabel();
            detailsLabel = new CenteredLabel();
            sizeLabel = new CenteredLabel();

            PackStart(BuildPreviewsStrip());
            PackStart(new HSeparator());
            PackStart(setNameLabel);
            PackStart(detailsLabel);
            PackStart(sizeLabel);

            WidthRequest = CalculateWidth(iconPreviews.Count);
            HorizontalPlacement = WidgetPlacement.Start;
            LoadLabels();
        }

        void LoadLabels()
        {
            var setInfo = images.First().Set;
            setNameLabel.Text = setInfo.Name;
            detailsLabel.Text = setInfo.Details;
            sizeLabel.Text = $"{setInfo.Size}{setInfo.Unit.GetName()}";
        }

        Widget BuildPreviewsStrip()
        {
            var container = new HBox {
                Spacing = itemSpacing,
                HorizontalPlacement = WidgetPlacement.Center,
            };

            foreach (var icon in images)
            {
                var preview = new IconItemPreview
                {
                    ImageName = icon.Scale.Name
                };

                iconPreviews.Add(preview);
                container.PackStart(preview);
            }

            return container;
        }

        int CalculateWidth(int quantity) => CalculatePreviewItemsSize(quantity) + horizontalMargin * 2;

        int CalculatePreviewItemsSize(int quantity)
        {
            if (quantity <= 0)
            {
                return 0;
            }
            if (quantity == 1)
            {
                return IconItemPreview.PreviewSize;
            }

            return (IconItemPreview.PreviewSize * quantity) + (itemSpacing * (quantity - 1));
        }

    }
}
