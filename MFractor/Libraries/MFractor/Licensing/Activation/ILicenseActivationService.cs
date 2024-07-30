using System;
using System.Threading.Tasks;

namespace MFractor.Licensing.Activation
{
    interface ILicenseActivationService
    {
        Task<ILicenseRequestResult> ActivateSerialKey(string emailAddress, string serialKey);

        Task<ILicenseRequestResult> ActivateTrialLicense(string emailAddress, string licenseeName);
    }
}
