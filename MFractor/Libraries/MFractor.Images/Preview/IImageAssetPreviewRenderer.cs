using System;
using System.ComponentModel.Composition;

namespace MFractor.Images.Preview
{
    [InheritedExport]
    public interface IImageAssetPreviewRenderer
    {
        bool CanProvidePreview(string filePath);

        ImageAssetPreview RenderPreview(string filePath);
    }
}