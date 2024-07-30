using MFractor.IOC;
using MFractor.Licensing;

namespace MFractor.Views.Branding
{
    /// <summary>
    /// A helper class for adding MFractors branding text.
    /// </summary>
    public static class BrandingHelper
    {
        public static Xwt.Drawing.Image BrandingIcon => Xwt.Drawing.Image.FromResource("mfractor_logo.png").WithSize(10, 10);

        /// <summary>
        /// The MFractor branding text that 
        /// </summary>
        /// <value>The branding text.</value>
        public static string BrandingText => Resolver.Resolve<ILicenseStatus>().BrandingText;

        /// <summary>
        /// Get's the <see cref="BrandingText"/> formatted as Pango markup for use in tooltips.
        /// </summary>
        /// <value>The pango formatted branding text.</value>
        public static string PangoFormattedBrandingText => $"<span size=\"x-small\" weight=\"bold\">{BrandingText}</span>";

        public static Xwt.Drawing.Font BrandingFontStyle => Xwt.Drawing.Font.SystemSansSerifFont.WithWeight(Xwt.Drawing.FontWeight.Bold).WithScaledSize(0.85);
    }
}
