using MFractor.Attributes;

namespace MFractor.Images
{
	/// <summary>
	/// See: https://developer.apple.com/ios/human-interface-guidelines/graphics/image-size-and-resolution/
	/// </summary>
	public enum AppleUnifiedImageDensities
    {
        /// <summary>
        /// The base image density, @1x.
        /// </summary>
        [Description("@1x")]
        [ScaleFactor(1.0)]
        Normal,

        /// <summary>
        /// The double image density, @2x.
        /// </summary>
        [Description("@2x")]
        [ScaleFactor(2.0)]
        Double,

        /// <summary>
        /// The triple image density, @3x.
        /// </summary>
        [Description("@3x")]
        [ScaleFactor(3.0)]
        Triple,
    }
}
