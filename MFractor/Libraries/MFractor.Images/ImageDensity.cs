using System;
using MFractor.Utilities;

namespace MFractor.Images
{
    /// <summary>
    /// An image density for a mobile asset.
    /// </summary>
    public class ImageDensity
	{
        /// <summary>
        /// The name of this image density.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// The scale of this image density.
        /// </summary>
        /// <value>The scale.</value>
        public double Scale { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Images.ImageDensity"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="scale">Scale.</param>
		public ImageDensity(string name, double scale)
		{
			Name = name;
			Scale = scale;
		}

        /// <summary>
        /// Creates an <see cref="ImageDensity"/> from the given <paramref name="value"/>.
        /// </summary>
        /// <returns>The enum value.</returns>
        /// <param name="value">Value.</param>
        public static ImageDensity FromEnumValue(Enum value)
		{
            var name = EnumHelper.GetDisplayValue(value).Item1;
            var scale = EnumHelper.GetScaleValue(value).Item1;

            return new ImageDensity(name, scale);
        }

        public override string ToString() => Name;
    }
}
