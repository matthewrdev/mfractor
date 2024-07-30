using System;

namespace MFractor.Licensing
{
    class LicenseRemovedEventArgs : EventArgs
    {
        public LicenseDetails Details { get; }

        public LicenseRemovedEventArgs(LicenseDetails details)
        {
            this.Details = details;
        }
    }
}

