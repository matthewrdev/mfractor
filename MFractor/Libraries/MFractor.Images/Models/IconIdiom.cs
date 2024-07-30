using System;
using System.Linq;
using System.Reflection;
using MFractor.Attributes;
using MFractor.Utilities;

namespace MFractor.Images.Models
{
    /// <summary>
    /// The Idioms in which an App Icon can be imported to projects.
    /// </summary>
    public enum IconIdiom
    {
        /// <summary>
        /// Idiom for Android Phones targets.
        /// </summary>
        [Description("Android Phone")]
        [ImportName("android")]
        AndroidPhone,

        /// <summary>
        /// Idiom for iOS application targets.
        /// </summary>
        [Description("iOS")]
        [ImportName("ios-marketing")]
        IOS,

        /// <summary>
        /// Idiom for iPhone application targets.
        /// </summary>
        [Description("iPhone")]
        [ImportName("iphone")]
        IPhone,

        /// <summary>
        /// Idiom for iPad application targets.
        /// </summary>
        [Description("iPad")]
        [ImportName("ipad")]
        IPad,

        /// <summary>
        /// Idiom for CarPlay application targets.
        /// </summary>
        [Description("CarPlay")]
        [ImportName("carplay")]
        CarPlay,
    }

    public static class IconIdiomExtensions
    {
        public static string GetDescription(this IconIdiom idiom) => EnumHelper.GetEnumDescription(idiom);

        public static string GetImportName(this IconIdiom idiom)
        {
            var fi = idiom.GetType().GetRuntimeField(idiom.ToString());
            var attributes = (ImportNameAttribute[])fi.GetCustomAttributes(typeof(ImportNameAttribute), false);
            return attributes?.First()?.Name;
        }
    }

}
