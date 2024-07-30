using System;
using MFractor.Licensing.Recovery;

namespace MFractor.Licensing.Recovery
{
    class LicenseRecoveryResult : ILicenseRecoveryResult
    {
        public LicenseRecoveryResult(string message, bool success)
        {
            Message = message;
            Success = success;
        }

        public string Message
        {
            get;
        }

        public bool Success
        {
            get;
        }
    }
}