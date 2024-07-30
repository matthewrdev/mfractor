using System;
using MFractor.Utilities;

namespace MFractor.Images.Preview.PreviewRenderers
{
    class BitmapPreviewRenderer : IImageAssetPreviewRenderer
    {
        public bool CanProvidePreview(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return ImageHelper.IsImageFile(filePath);
        }

        public ImageAssetPreview RenderPreview(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}