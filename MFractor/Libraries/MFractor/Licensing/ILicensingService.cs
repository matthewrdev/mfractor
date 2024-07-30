using System;

namespace MFractor.Licensing
{
    public interface ILicensingService
    {
        bool HasActivation { get; }

        string ActivationEmail { get; }

        string ActivationName { get; }

        bool IsPaid { get; }

        bool IsTrial { get; }

        LicenseDetails LicensingDetails { get; }

        bool IsUsingLegacyLicense { get; }

        bool HasStarted { get; }

        bool HasLicense { get; }

        event EventHandler<EventArgs> LicenseServiceStarted;

        LicenseStatusMessage GetLicenseStatusMessage();
    }
}