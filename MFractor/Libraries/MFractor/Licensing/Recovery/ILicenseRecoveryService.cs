using System;
using System.Threading.Tasks;

namespace MFractor.Licensing.Recovery
{
    interface ILicenseRecoveryService
    {
        Task<ILicenseRecoveryResult> RecoverLicense(string emailAddress);
    }
}
