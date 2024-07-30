using System;
using System.Collections.Generic;
using Portable.Licensing;

namespace MFractor.Licensing.Issuer
{
    public class LicenseIssuer
    {
        public LicenseConfig Config { get; }
        public LicenseSigningConfig Signing { get; }

        public LicenseIssuer(LicenseConfig config, LicenseSigningConfig signingConfig)
        {
            Config = config;
            Signing = signingConfig;
        }

        public string Issue()
        {
            var metaData = Config.MetaData ?? new Dictionary<string, string>();

            metaData.Add("sku", Config.ProductSku);

            var license = License.New()
                            .WithUniqueIdentifier(Config.Guid)
                            .ExpiresAt(Config.Expiry)
                            .As(Config.IsTrial ? LicenseType.Trial : LicenseType.Standard)
                            .WithMaximumUtilization(Config.MaxUtilisations)
                            .LicensedTo(Config.Name, Config.Email)
                            .WithAdditionalAttributes(metaData)
                            .CreateAndSignWithPrivateKey(Signing.PrivateKey, Signing.Passphrase);

            return Utils.Base64Encode(license.ToString());
        }
    }
}