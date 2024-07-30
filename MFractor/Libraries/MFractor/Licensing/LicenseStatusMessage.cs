using System;

namespace MFractor.Licensing
{
    public class LicenseStatusMessage
    {
        public string Title { get; }

        public string Message { get; }

        public LicenseStatusMessageKind LicenseStatusMessageKind { get; }

        public LicenseStatusMessage(string title,
                                    string message,
                                    LicenseStatusMessageKind licenseStatusMessageKind)
        {
            Title = title;
            Message = message;
            LicenseStatusMessageKind = licenseStatusMessageKind;
        }
    }
}

