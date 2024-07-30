using System;
using System.ComponentModel.Composition;

namespace MFractor.Licensing
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILicenseStatus))]
    class LicenseStatus : ILicenseStatus
    {
        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        [ImportingConstructor]
        public LicenseStatus(Lazy<ILicensingService> licensingService)
        {
            this.licensingService = licensingService;
        }

        public string BrandingText
        {
            get
            {
                if (LicensingService.IsPaid)
                {
                    if (LicensingService.IsTrial)
                    {
                        return "Powered By MFractor Professional (Trial)";
                    }

                    return "Powered By MFractor Professional";
                }

                return "Powered By MFractor Lite";
            }
        }
    }
}