using System.Collections.Generic;
using System.Linq;
using MFractor.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Models
{
    /// <summary>
    /// Represents a set of Icon Groups that are importable to a specific device.
    /// </summary>
    public class AppIconSet : ObservableBase
    {
        /// <summary>
        /// The name of this Device Icon Group.
        /// </summary>
        public string Name { get; set; }

        bool isSelected = true;
        /// <summary>
        /// If the property is selected.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                SetProperty(ref isSelected, value);
                OnPropertyChanged(nameof(IconCount));
            }
        }

        /// <summary>
        /// The count of icons in this group.
        /// </summary>
        public int IconCount => IsSelected ? Images.Count() : 0;

        /// <summary>
        /// Gets all the images that composes this device icon group.
        /// </summary>
        public IEnumerable<IconImage> Images { get; set; }

        public static IEnumerable<IconImage> AndroidIcons()
        {
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidLauncher, ImageScale.DP_LDPI);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidLauncher, ImageScale.DP_MDPI);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidLauncher, ImageScale.DP_HDPI);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidLauncher, ImageScale.DP_XHDPI);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidLauncher, ImageScale.DP_XXHDPI);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidLauncher, ImageScale.DP_XXXHDPI);
        }

        public static IEnumerable<IconImage> AndroidAdaptiveIcons()
        {
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidAdaptiveLauncher, ImageScale.DP_MDPI, true);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidAdaptiveLauncher, ImageScale.DP_HDPI, true);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidAdaptiveLauncher, ImageScale.DP_XHDPI, true);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidAdaptiveLauncher, ImageScale.DP_XXHDPI, true);
            yield return new IconImage(IconIdiom.AndroidPhone, IconSet.AndroidAdaptiveLauncher, ImageScale.DP_XXXHDPI, true);
        }

        public static IEnumerable<IconImage> IOSIcons()
        {
            yield return new IconImage(IconIdiom.IOS, IconSet.AppStore, ImageScale.Points_1x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.Notification, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.Notification, ImageScale.Points_3x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.Settings, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.Settings, ImageScale.Points_3x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.Spotlight, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.Spotlight, ImageScale.Points_3x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.IPhoneApp, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPhone, IconSet.IPhoneApp, ImageScale.Points_3x);
            yield return new IconImage(IconIdiom.IPad, IconSet.Notification, ImageScale.Points_1x);
            yield return new IconImage(IconIdiom.IPad, IconSet.Notification, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPad, IconSet.Settings, ImageScale.Points_1x);
            yield return new IconImage(IconIdiom.IPad, IconSet.Settings, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPad, IconSet.Spotlight, ImageScale.Points_1x);
            yield return new IconImage(IconIdiom.IPad, IconSet.Spotlight, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPad, IconSet.IPadApp, ImageScale.Points_1x);
            yield return new IconImage(IconIdiom.IPad, IconSet.IPadApp, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.IPad, IconSet.IPadProApp, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.CarPlay, IconSet.CarPlay, ImageScale.Points_2x);
            yield return new IconImage(IconIdiom.CarPlay, IconSet.CarPlay, ImageScale.Points_3x);
        }

    }
}
