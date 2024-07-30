using System;
namespace MFractor.Licensing
{
    class LicenseConfig
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string License { get; set; }

        public bool Activated => string.IsNullOrEmpty(Email) == false;

        public bool HasLicense => !string.IsNullOrEmpty(License);

        public bool HasName => !string.IsNullOrEmpty(Name);
    }
}
