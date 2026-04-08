using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Versioning;

namespace MFractor.Rider
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProductInformation))]
    public class MFractorRiderProductInformation : IProductInformation
    {
        static SemanticVersion CreateVersion(Version version)
        {
            if (version == null)
            {
                return new SemanticVersion(0, 1, 0, 0);
            }

            return new SemanticVersion(
                Math.Max(version.Major, 0),
                Math.Max(version.Minor, 0),
                Math.Max(version.Build, 0),
                Math.Max(version.Revision, 0));
        }

        public SemanticVersion Version { get; } = CreateVersion(typeof(MFractorRiderProductInformation).Assembly.GetName().Version);

        public string ProductName => "MFractor";

        public string ProductVariant => "Rider";

        public SemanticVersion ExternalProductVersion { get; } = new SemanticVersion(2025, 3, 0);

        public Product Product => Product.Rider;

        public string UtmSource => "rider";

        public string SKU => "MFRACTOR-RIDER";

        public string VersionMarketingUrl => string.Empty;

        public IReadOnlyList<string> InstalledExtensions { get; } = Array.Empty<string>();

        public string RuntimeVersion => Environment.Version.ToString();
    }
}
