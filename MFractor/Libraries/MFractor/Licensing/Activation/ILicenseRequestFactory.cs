using System;

namespace MFractor.Licensing.Activation
{
    interface ILicenseRequestFactory
    {
        LicenseRequest CreateSerialKeyRequest(string emailAddress, string serialKey);

        LicenseRequest CreateTrialLicenseRequest(string emailAddress, string name);
    }
}