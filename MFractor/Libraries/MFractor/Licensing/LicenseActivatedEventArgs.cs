using System;

namespace MFractor.Licensing
{
    class LicenseActivatedEventArgs : EventArgs
    {
        public LicenseDetails Details { get; }

        public LicenseActivatedEventArgs(LicenseDetails details)
        {
            Details = details;
        }
    }
}

