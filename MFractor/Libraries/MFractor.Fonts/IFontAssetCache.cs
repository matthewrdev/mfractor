using System;

namespace MFractor.Fonts
{
    public interface IFontAssetCache
    {
        IFont TryGetFontAsset(string fontAssetUrl);
    }
}
