using System;
using System.Collections.Generic;
using System.IO;

namespace MFractor.Licensing
{
    /// <summary>
    /// An <see cref="ILicensingService"/> that allows editing of the installed MFractor license.
    /// </summary>
    interface IMutableLicensingService : ILicensingService
    {
        event EventHandler<LicenseActivatedEventArgs> OnLicenseActivated;

        event EventHandler<LicenseRemovedEventArgs> OnLicenseRemoved;

        void Activate(LicensedUserInformation options);

        bool ImportLicense(FileInfo license, out IReadOnlyList<string> issues);

        bool ImportLicense(string licenseContent, out IReadOnlyList<string> issues);

        bool RemoveActiveLicense(out IReadOnlyList<string> issues);
    }
}