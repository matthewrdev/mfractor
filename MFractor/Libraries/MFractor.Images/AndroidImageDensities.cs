using System;
using MFractor.Attributes;

namespace MFractor.Images
{
	/// <summary>
	/// See: https://developer.android.com/guide/practices/screens_support.html
	/// </summary>
	public enum AndroidImageDensities
	{
        /// <summary>
        /// The ldpi image density.
        /// </summary>
        [Description("ldpi")]
		[ScaleFactor(0.75)]
		Low,

        /// <summary>
        /// The mdpi image density.
        /// </summary>
        [Description("mdpi")]
		[ScaleFactor(1.0)]
		Medium,

        /// <summary>
        /// The hdpi image density.
        /// </summary>
        [Description("hdpi")]
		[ScaleFactor(1.5)]
		High,

        /// <summary>
        /// The xhdpi image density.
        /// </summary>
        [Description("xhdpi")]
		[ScaleFactor(2.0)]
		ExtraHigh,

        /// <summary>
        /// The xxhdpi image density.
        /// </summary>
        [Description("xxhdpi")]
		[ScaleFactor(3.0)]
		ExtraExtraHigh,

        /// <summary>
        /// The xxxhdpi image density.
        /// </summary>
        [Description("xxxhdpi")]
		[ScaleFactor(4.0)]
		ExtraExtraExtraHigh,
	}
}
