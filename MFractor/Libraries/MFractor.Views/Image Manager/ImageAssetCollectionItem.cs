using System;
using System.IO;
using MFractor.Images;
using MFractor.Utilities;
using MFractor.Views.Controls.Collection;
using Xwt.Drawing;

namespace MFractor.Views.ImageManager
{
    public class ImageAssetCollectionItem : ICollectionItem, IDisposable
    {
        const int previewSize = 32;

        Image placeholderImage;

        public ImageAssetCollectionItem(IImageAsset imageAsset,
                                        Image placeholderImage)
        {
            ImageAsset = imageAsset ?? throw new ArgumentNullException(nameof(imageAsset));
            this.placeholderImage = placeholderImage ?? throw new ArgumentNullException(nameof(placeholderImage));

            try
            {
                if (File.Exists(imageAsset.PreviewImageFilePath)
                    && ImageHelper.IsImageFile(imageAsset.PreviewImageFilePath))
                {
                    icon = Image.FromFile(imageAsset.PreviewImageFilePath).WithSize(previewSize, previewSize);
                }
            }
            catch (Exception)
            {
                // Suppress, Icon will fallback to the placeholder image.
            }
        }

        readonly Image icon;
        public Image Icon => icon ?? placeholderImage;

        public string DisplayText => ImageAsset.Name;

        public string SearchText => ImageAsset.SearchName;

        public bool IsChecked
        {
            get;
            set;
        }
        public IImageAsset ImageAsset { get; }

        public string SecondaryDisplayText => string.Empty;

        public void Dispose()
        {
            placeholderImage = null;
            icon?.Dispose();
        }
    }
}
