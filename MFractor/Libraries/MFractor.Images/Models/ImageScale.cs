using System;

namespace MFractor.Images.Models
{
    /// <summary>
    /// Describes a Mobile Device Image Scale properties.
    /// </summary>
    public sealed class ImageScale : IEquatable<ImageScale>
    {
        /// <summary>
        /// The Name that describes this scale.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The Unit of the scale.
        /// </summary>
        public ScaleUnit Unit { get; private set; }

        /// <summary>
        /// The multiplication factor for the base unit size of this scale descriptor.
        /// </summary>
        public double Factor { get; private set; }

        ImageScale()
        {
        }

        public override bool Equals(object obj) => Equals(obj as ImageScale);

        public bool Equals(ImageScale other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (GetType() != other.GetType())
            {
                return false;
            }

            return (Name == other.Name)
                && (Factor == other.Factor)
                && (Unit == other.Unit);
        }

        public override int GetHashCode() => Name.GetHashCode() + Factor.GetHashCode() + Unit.GetHashCode();

        public static bool operator ==(ImageScale lhs, ImageScale rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(ImageScale lhs, ImageScale rhs) => !(lhs == rhs);

        public bool IsAppleScale => Unit == ScaleUnit.Points;

        public bool IsAndroidScale => Unit == ScaleUnit.DP;

        /// <summary>
        /// Gets all the Android specific scales.
        /// </summary>
        public static ImageScale[] AndroidScales => new[] { DP_LDPI, DP_MDPI, DP_HDPI, DP_XHDPI, DP_XXHDPI, DP_XXXHDPI, };

        /// <summary>
        /// Gets all the iOS specific scales.
        /// </summary>
        public static ImageScale[] IOSScales => new[] { Points_1x, Points_2x, Points_3x, };

        #region iOS Scales

        /// <summary>
        /// Gets the descriptor for the iOS 1x pt scale.
        /// </summary>
        public static ImageScale Points_1x => new ImageScale
        {
            Name = "1x",
            Factor = 1.0,
            Unit = ScaleUnit.Points,
        };

        /// <summary>
        /// Gets the descriptor for the iOS 2x pt scale.
        /// </summary>
        public static ImageScale Points_2x => new ImageScale
        {
            Name = "2x",
            Factor = 2.0,
            Unit = ScaleUnit.Points,
        };

        /// <summary>
        /// Gets the descriptor for the iOS 3x pt scale.
        /// </summary>
        public static ImageScale Points_3x => new ImageScale
        {
            Name = "3x",
            Factor = 3.0,
            Unit = ScaleUnit.Points,
        };

        #endregion


        #region Android Scales

        /// <summary>
        /// Gets the descriptor for the Android ldpi scale.
        /// </summary>
        public static ImageScale DP_LDPI => new ImageScale
        {
            Name = "ldpi",
            Factor = 0.75,
            Unit = ScaleUnit.DP,
        };

        /// <summary>
        /// Gets the descriptor for the Android mdpi scale.
        /// </summary>
        public static ImageScale DP_MDPI => new ImageScale
        {
            Name = "mdpi",
            Factor = 1.0,
            Unit = ScaleUnit.DP,
        };

        /// <summary>
        /// Gets the descriptor for the Android hdpi scale.
        /// </summary>
        public static ImageScale DP_HDPI => new ImageScale
        {
            Name = "hdpi",
            Factor = 1.5,
            Unit = ScaleUnit.DP,
        };

        /// <summary>
        /// Gets the descriptor for the Android xhdpi scale.
        /// </summary>
        public static ImageScale DP_XHDPI => new ImageScale
        {
            Name = "xhdpi",
            Factor = 2.0,
            Unit = ScaleUnit.DP,
        };

        /// <summary>
        /// Gets the descriptor for the Android xxhdpi scale.
        /// </summary>
        public static ImageScale DP_XXHDPI => new ImageScale
        {
            Name = "xxhdpi",
            Factor = 3.0,
            Unit = ScaleUnit.DP,
        };

        /// <summary>
        /// Gets the descriptor for the Android xxxhdpi scale.
        /// </summary>
        public static ImageScale DP_XXXHDPI => new ImageScale
        {
            Name = "xxxhdpi",
            Factor = 4.0,
            Unit = ScaleUnit.DP,
        };

        #endregion
    }
}
