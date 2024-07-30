using System;
using MFractor.Maui.Symbols;
using MFractor.Images;

namespace MFractor.Editor.Tooltips
{
    [Obsolete("The image tooltip model is obsolete and should be replaced with Microsoft.VisualStudio.Text.Adornments.ImageElement.")]
    public class ImageTooltipModel
    {
        public IImageAsset ImageAsset { get; }

        public ImageTooltipModel(IImageAsset imageAsset)
        {
            ImageAsset = imageAsset;
        }
    }
}
