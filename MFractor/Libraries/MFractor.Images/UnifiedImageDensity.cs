using System;
using MFractor.Attributes;
using MFractor.Images.Attributes;

namespace MFractor.Images
{
    /// <summary>
    /// See: https://developer.android.com/guide/practices/screens_support.html
    /// </summary>
    public enum UnifiedImageDensityKind
    {
        /// <summary>
        /// The image density factor is unknown or not applicable.
        /// <para/>
        /// This applies if the image is a PDF, vector or android drawable.
        /// </summary>
        Unknown,

        /// <summary>
        /// A density factor that is 0.75x.
        /// </summary>
        [Description("@.75x")]
        [ImageDensityName("ldpi", PlatformFramework.Android)]
        [ScaleFactor(0.75)]
        PointSevenFive,

        /// <summary>
        /// A density factor that is 1x.
        /// </summary>
        [Description("@1x")]
        [ImageDensityName("@1x", PlatformFramework.iOS)]
        [ImageDensityName("mdpi", PlatformFramework.Android)]
        [ScaleFactor(1.0)]
        One,

        /// <summary>
        /// A density factor that is 1.5x.
        /// </summary>
        [Description("@1.5x")]
        [ImageDensityName("hdpi", PlatformFramework.Android)]
        [ScaleFactor(1.5)]
        OnePointFive,

        /// <summary>
        /// A density factor that is 2x.
        /// </summary>
        [Description("@2x")]
        [ImageDensityName("@2x", PlatformFramework.iOS)]
        [ImageDensityName("xhdpi", PlatformFramework.Android)]
        [ScaleFactor(2.0)]
        Two,

        /// <summary>
        /// A density factor that is 3x.
        /// </summary>
        [Description("xxhdpi")]
        [ImageDensityName("@3x", PlatformFramework.iOS)]
        [ImageDensityName("xxhdpi", PlatformFramework.Android)]
        [ScaleFactor(3.0)]
        Three,

        /// <summary>
        /// A density factor that is 4x.
        /// </summary>
        [ImageDensityName("xxxhdpi", PlatformFramework.Android)]
        [ScaleFactor(4.0)]
        Four,
    }
}
