using System.Reflection;
using System.Text;
using MFractor.Licensing;
using MFractor.Versioning;
using MFractor.VS.Mac;
using Mono.Addins;

[assembly:Addin (
    "MFractor",
    Namespace = "MFractor.VS.Mac",
    Version = ProductInfo.ProductVersion
)]

[assembly: AddinName ("MFractor")]
[assembly: AddinCategory ("IDE extensions")]
[assembly: AddinDescription ("The ultimate productivity tool for .NET MAUI.")]
[assembly: AddinAuthor ("Matthew Robbins")]
[assembly: AddinUrl ("https://www.mfractor.com")]

namespace MFractor.VS.Mac
{
    public static class ProductInfo
    {
        public const string ProductName = "MFractor";
        public const string ProductVersion = "5.2.0";
        public const string ProductVariant = "Visual Studio Mac";
        public const string ProductSKU = "MFRACTOR-VS-MAC";
        public const string UtmSource = "vs_mac";

        internal const string PrimarySigningKey = "REDACTED";
        internal const string SecondarySigningKey = "REDACTED";

        public const string VersionMarketingUrl = "https://www.mfractor.com/blogs/news/introducing-ansight";

        internal static readonly ProductLicensingInfo ProductLicensingInfo = new ProductLicensingInfo(ProductSKU, Encoding.UTF8.GetBytes(PrimarySigningKey), Encoding.UTF8.GetBytes(SecondarySigningKey));
        public static readonly SemanticVersion SemanticVersion = SemanticVersion.Parse(ProductVersion);
    }
}
