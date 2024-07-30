using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using MFractor.Versioning;
using Mono.Addins;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProductInformation))]
    class ProductInformation : IProductInformation
    {
        public string ProductName => ProductInfo.ProductName;

        public string ProductVariant => ProductInfo.ProductVariant;

        public SemanticVersion ExternalProductVersion => SemanticVersion.Parse(IdeApp.Version.ToString());

        public Product Product => Product.VisualStudioMac;

        public string UtmSource => ProductInfo.ProductVariant;

        public string SKU => ProductInfo.ProductSKU;

        public string VersionMarketingUrl => ProductInfo.VersionMarketingUrl;

        public SemanticVersion Version => SemanticVersion.Parse(ProductInfo.ProductVersion);

        public IReadOnlyList<string> InstalledExtensions => AddinManager.Registry.GetAddins().Select(a => $"{a.Name} ({a.Version})").ToList();

        public string RuntimeVersion
        {
            get
            {
                var type = Type.GetType("Mono.Runtime");
                if (type != null)
                {
                    var getDisplayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (getDisplayName != null)
                    {
                        return (string)getDisplayName.Invoke(null, null);
                    }
                }

                return string.Empty;
            }
        }
    }
}
