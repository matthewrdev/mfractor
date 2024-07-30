using System;

namespace MFractor.Images.Models
{
    /// <summary>
    /// Describes information of a set of application icons of an Idiom.
    /// Each Idiom has one or more Icon Sets. An icon set is used for specific
    /// purpose on the application (such as launch screen, notifications,
    /// settings, etc.).
    /// </summary>
    public class IconSet : IEquatable<IconSet>
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name to se when exporting the icon.
        /// </summary>
        public string ImportName { get; set; }

        /// <summary>
        /// The details for the group.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// The size of the Icon. Since icons are square images, the width is equals to the height.
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// The base unit for this descriptor.
        /// </summary>
        public ScaleUnit Unit { get; set; }

        IconSet()
        {
        }

        public override int GetHashCode() => (Name.GetHashCode() + Details.GetHashCode() + ImportName.GetHashCode() + Size.GetHashCode() * 0x00010000);

        public override bool Equals(object obj) => Equals(obj as IconSet);

        public bool Equals(IconSet other) => other.Name == Name &&
                other.Details == Details &&
                other.Size == Size &&
                other.Unit == Unit;

        public static bool operator ==(IconSet lhs, IconSet rhs)
        {
            // Check for null on left side.
            if (object.ReferenceEquals(lhs, null))
            {
                if (object.ReferenceEquals(null, rhs))
                {
                    return true;
                }
                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(IconSet lhs, IconSet rhs) => !(lhs == rhs);

        #region Defined Descriptors

        public static IconSet AndroidLauncher => new IconSet
        {
            Name = "Launcher Icons",
            ImportName = "launcher",
            Details = "Android Launcher Icons",
            Size = 48,
            Unit = ScaleUnit.DP,
        };

        public static IconSet AndroidAdaptiveLauncher => new IconSet
        {
            Name = "Adaptive Launcher Icons",
            ImportName = "adaptive launcher",
            Details = "Android Adaptive Launcher Icons",
            Size = 108,
            Unit = ScaleUnit.DP,
        };

        public static IconSet AppStore => new IconSet
        {
            Name = "App Store",
            ImportName = "appstore",
            Details = "iOS",
            Size = 1024,
            Unit = ScaleUnit.Points,
        };

        public static IconSet Notification => new IconSet
        {
            Name = "Notification",
            ImportName = "notifications",
            Details = "iOS 7-14",
            Size = 20,
            Unit = ScaleUnit.Points,
        };

        public static IconSet Settings => new IconSet
        {
            Name = "Settings",
            ImportName = "settings",
            Details = "iOS 7-14",
            Size = 29,
            Unit = ScaleUnit.Points,
        };

        public static IconSet Spotlight => new IconSet
        {
            Name = "Spotlight",
            ImportName = "spotlight",
            Details = "iOS 7-14",
            Size = 40,
            Unit = ScaleUnit.Points,
        };

        public static IconSet IPhoneApp => new IconSet
        {
            Name = "App",
            ImportName = "app",
            Details = "iOS 7-14",
            Size = 60,
            Unit = ScaleUnit.Points,
        };

        public static IconSet IPadApp => new IconSet
        {
            Name = "App",
            ImportName = "app",
            Details = "iOS 7-14",
            Size = 76,
            Unit = ScaleUnit.Points,
        };

        public static IconSet IPadProApp => new IconSet
        {
            Name = "iPad Pro (12.9-inch) App",
            ImportName = "pro-app",
            Details = "iOS 9-14",
            Size = 83.5,
            Unit = ScaleUnit.Points,
        };

        public static IconSet CarPlay => new IconSet
        {
            Name = "CarPlay",
            ImportName = "carplay",
            Details = "60pt",
            Size = 60,
            Unit = ScaleUnit.Points,
        };

        #endregion
    }
}
