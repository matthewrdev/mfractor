using System;
using System.Collections.Generic;

namespace MFractor.Images
{
    public interface IImageAssetCollection : IReadOnlyDictionary<string, IImageAsset>
    {
    }
}