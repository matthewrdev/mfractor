using MFractor.Attributes;

namespace MFractor
{
    /// <summary>
    /// The outer product that MFractor is currently integrated into.
    /// </summary>
    public enum Product
    {
        /// <summary>
        /// The IDE is Visual Studio Windows.
        /// </summary>
        [Description("MFRACTOR-VS-WINDOWS")]
        VisualStudioWindows,

        /// <summary>
        /// The IDE is Visual Studio for Mac.
        /// </summary>
        [Description("MFRACTOR-VS-MAC")]
        VisualStudioMac,

        /// <summary>
        /// The IDE is Jetbrains Rider.
        /// </summary>
        [Description("MFRACTOR-RIDER")]
        Rider,

        /// <summary>
        /// The IDE is Visual Studio Code.
        /// </summary>
        [Description("MFRACTOR-VS-CODE")]
        VisualStudioCode,

        /// <summary>
        /// The IDE is XCode.
        /// </summary>
        [Description("MFRACTOR-XCODE")]
        XCode,

        /// <summary>
        /// The IDE is Android Studio.
        /// </summary>
        [Description("MFRACTOR-ANDROID-STUDIO")]
        AndroidStudio,

        /// <summary>
        /// The product is MFractor.CLI, MFractors command line utility.
        /// </summary>
        [Description("MFRACTOR-CLI")]
        CLI,

        /// <summary>
        /// The product is Image Asset Studio, MFractors command line utility.
        /// </summary>
        [Description("MFRACTOR-IMAGE-STUDIO")]
        ImageStudio,

        /// <summary>
        /// The product is Simulator Manager for iOS, MFractors command line utility.
        /// </summary>
        [Description("MFRACTOR-CLI")]
        IOSSimulatorManager,
    }
}
