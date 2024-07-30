using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Images.Preview
{
    public interface IImageAssetPreviewRendererRepository : IPartRepository<IImageAssetPreviewRenderer>
    {
        IReadOnlyList<IImageAssetPreviewRenderer> Renderers { get; }
    }
}