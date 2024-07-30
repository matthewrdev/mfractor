using System;
namespace MFractor.Licensing
{
    /// <summary>
    /// The public signing keys used to decypt a product license.
    /// </summary>
    interface ILicenseSigningInformation
    {
        /// <summary>
        /// The public signing key used to verify MFractor licenses.
        /// </summary>
        string PrimarySigningKey { get; }

        /// <summary>
        /// An alternative signing key that can be used to verify MFractor licenses.
        /// <para/>
        /// If a product can be purchased as a standalone or is automatically granted to holders of "bundle" license, this signing key allows users to activate using that.
        /// </summary>
        string SecondarySigningKey { get; }

        /// <summary>
        /// The licensing information.
        /// </summary>
        ProductLicensingInfo ProductLicensingInfo { get; }
    }
}
