using System;
using System.IO;

namespace MFractor.Images.Preview.PreviewRenderers
{
    class PdfPreviewRenderer : IImageAssetPreviewRenderer
    {
        public bool CanProvidePreview(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return Path.GetExtension(filePath) == ".pdf";
        }

        public ImageAssetPreview RenderPreview(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}