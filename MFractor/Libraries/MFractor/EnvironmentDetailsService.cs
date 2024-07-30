using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IEnvironmentDetailsService))]
    class EnvironmentDetailsService : IEnvironmentDetailsService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IProductInformation> productInformation;
        IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IPlatformService> platformService;
        IPlatformService PlatformService => platformService.Value;

        [ImportingConstructor]
        public EnvironmentDetailsService(Lazy<IProductInformation> productInformation,
                                         Lazy<IPlatformService> platformService)
        {
            this.productInformation = productInformation;
            this.platformService = platformService;
        }

        public IReadOnlyList<string> Details
        {
            get
            {
                var result = new List<string>
                {
                    "--Platform Information--",
                    $"Operating System: {PlatformService.Name}",
                    "OS: " + Environment.OSVersion.Platform.ToString(),
                    "OS Service Pack: " + Environment.OSVersion.ServicePack,
                    "OS Version: " + Environment.OSVersion.VersionString,
                    "--MFractor Information--",
                    "Product Name: " + ProductInformation.ProductName,
                    "Product Version: " + ProductInformation.Version.ToString(),
                    "Product Variant: " + ProductInformation.ProductVariant,
                    "Product Variant Version: " + ProductInformation.ExternalProductVersion,
                    "Product SKU: " + ProductInformation.SKU,
                    "Installed extensions:\t" + string.Join(", ", ProductInformation.InstalledExtensions.Select(extensionName => "'" + extensionName + "'"))
                };

                return result;
            }
        }

        public void Startup()
        {
            foreach (var detail in Details)
            {
                log.Info(detail);
            }
        }

        public void Shutdown()
        {
        }
    }
}