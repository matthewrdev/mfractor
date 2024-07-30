using MFractor.Attributes;

namespace MFractor
{
    /// <summary>
    /// A unique platform or framework.
    /// </summary>
    public enum PlatformFramework
    {
        /// <summary>
        /// The Android platform.
        /// </summary>
        [Description("Android")]
        Android,

        /// <summary>
        /// The iOS platform.
        /// </summary>
        [Description("iOS")]
        iOS,

        /// <summary>
        /// The WatchOS platform.
        /// </summary>
        [Description("WatchOS")]
        WatchOS,

        /// <summary>
        /// The TVOS platform.
        /// </summary>
        [Description("TVOS")]
        TVOS,

        /// <summary>
        /// The MacOS platform.
        /// </summary>
        [Description("MacOS")]
        MacOS,

        /// <summary>
        /// The Windows Presentation Framework.
        /// </summary>
        [Description("WPF")]
        WPF,

        /// <summary>
        /// The Universal Windows Platform
        /// </summary>
        [Description("UWP")]
        UWP,

        /// <summary>
        /// Tizen, Samsungs mobile operating system.
        /// </summary>
        [Description("Tizen")]
        Tizen,

        /// <summary>
        /// The platform is unknown.
        /// </summary>
        [Description("NA")]
        Unknown = -1,
    }
}
