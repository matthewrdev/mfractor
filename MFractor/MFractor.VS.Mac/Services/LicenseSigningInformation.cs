using System;
using System.ComponentModel.Composition;
using MFractor.Licensing;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILicenseSigningInformation))]
    class LicenseSigningInformation : ILicenseSigningInformation
    {
        public string PrimarySigningKey => ProductInfo.PrimarySigningKey;

        public string SecondarySigningKey => ProductInfo.SecondarySigningKey;

        public ProductLicensingInfo ProductLicensingInfo => ProductInfo.ProductLicensingInfo;
    }
}