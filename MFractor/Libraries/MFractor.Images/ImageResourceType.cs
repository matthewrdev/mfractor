using System;
using MFractor.Attributes;

namespace MFractor.Images
{
    public enum ImageResourceType
    {
        [Description("Bundle Resource")]
        BundleResource,

        [Description("Asset Catalog")]
        AssetCatalog,

        [Description("Drawable")]
        Drawable,

        [Description("MipMap")]
        MipMap,

        [Description("Shared Image")]
        SharedImage,

        [Description("Embedded Resource")]
        EmbeddedResource,

        [Description("Content")]
        Content,

        [Description("MAUI Image Asset")]
        MauiImage,

        [Description("MAUI App Icon")]
        MauiAppIcon,

    }
}
