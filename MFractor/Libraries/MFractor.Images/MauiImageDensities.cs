using System;
using MFractor.Attributes;

namespace MFractor.Images
{
    public enum MauiImageDensities
    {
        /// <summary>
        /// The base image density, @1x.
        /// </summary>
        [Description("Maui Image Asset")]
        [ScaleFactor(1.0)]
        Normal,
    }
}

