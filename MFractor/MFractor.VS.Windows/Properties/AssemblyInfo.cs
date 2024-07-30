using System.Reflection;
using System.Text;
using MFractor.Licensing;
using MFractor.Versioning;

[assembly: AssemblyTitle("MFractor.VS.Windows")]
[assembly: AssemblyProduct("MFractor.VS.Windows")]
[assembly: AssemblyVersion(MFractor.VS.Windows.ProductInfo.ProductVersion + "." + MFractor.VS.Windows.ProductInfo.BuildNumberFormatted)]
[assembly: AssemblyCompany("MFractor Pty Ltd")]
[assembly: AssemblyCopyright("MFractor Pty Ltd")]
[assembly: AssemblyTrademark("MFractor Pty Ltd")]
[assembly: AssemblyDescription("A powerful productivity tool for Xamarin developers.")] 
namespace MFractor.VS.Windows
{
    public static class ProductInfo
    {
        public const string ProductName = "MFractor";
        public const string ProductVersion = Vsix.Version;
        public const string ProductVariant = "Visual Studio For Windows";
        public const string ProductSKU = "MFRACTOR-VS-WINDOWS";
        public const string BuildNumberFormatted = "29500";
        internal const string PrimarySigningKey = "REDACTED";
        internal const string SecondarySigningKey = "REDACTED";
        public const string VersionReleaseUrl = "https://www.mfractor.com/blogs/news/introducing-the-app-icon-importer";
        public const string GitCommitSHA = "456a155ffb822243ae788e1ff912fa28be4af2e3";
        public const string GitBranch = "master";
        public const int GitRevision = 5000;
        public const string GitTag = "";
        public const string BuildDate = "18/03/2022 2:26:45 AM (UTC)";
        public const string BuildAgent = "Local";

        internal static readonly ProductLicensingInfo ProductLicensingInfo = new ProductLicensingInfo(ProductSKU, Encoding.UTF8.GetBytes(PrimarySigningKey), Encoding.UTF8.GetBytes(SecondarySigningKey));
        public static readonly SemanticVersion SemanticVersion = SemanticVersion.Parse(ProductVersion);
    }
}
