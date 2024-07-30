using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Licensing;
using MFractor.Versioning;
using MFractor.VS.Windows.Utilities;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProductInformation))]
    class ProductInformation : IProductInformation
    {
        public string ProductName => ProductInfo.ProductName;

        public string ProductVersion => Vsix.Version;

        public string ProductVariant => ProductInfo.ProductVariant;

        public SemanticVersion ExternalProductVersion => SemanticVersion.Parse(IdeEnvironmentDetails.IdeVersion);

        public string UtmSource => ProductInfo.ProductVariant;

        public string SKU => ProductInfo.ProductSKU;

        public string PublicSigningKey => ProductInfo.PrimarySigningKey;

        public string LegacySigningKey => ProductInfo.SecondarySigningKey;

        public string VersionMarketingUrl => ProductInfo.VersionReleaseUrl;

        public string GitCommitSHA => ProductInfo.GitCommitSHA;

        public string GitBranch => ProductInfo.GitBranch;

        public int GitRevision => ProductInfo.GitRevision;

        public string GitTag => ProductInfo.GitTag;

        public string BuildDate => ProductInfo.BuildDate;

        public string BuildAgent => ProductInfo.BuildAgent;

        public ProductLicensingInfo ProductLicensingInfo => ProductInfo.ProductLicensingInfo;

        public SemanticVersion Version => SemanticVersion.Parse(ProductVersion);

        public IReadOnlyList<string> InstalledExtensions => IdeEnvironmentDetails.Extensions.ToList();

        public string RuntimeVersion => System.Environment.Version.ToString();

        public Product Product => Product.VisualStudioWindows;
    }
}
