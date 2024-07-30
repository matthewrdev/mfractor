using System;
using MFractor.Licensing;


namespace MFractor.Licensing.Activation
{
    class LicenseRequestResult : ILicenseRequestResult
    {
        public LicenseRequestResult(bool success,
                                    string licenseContent,
                                    string statusMessage,
                                    string statusDetail)
        {
            Success = success;
            LicenseContent = licenseContent;
            StatusMessage = statusMessage;
            StatusDetail = statusDetail;
        }

        public bool Success { get; }

        public string LicenseContent { get; }

        public string StatusMessage { get; }

        public string StatusDetail { get; }
    }
}
