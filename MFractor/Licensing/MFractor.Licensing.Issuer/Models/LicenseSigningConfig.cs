using System;
using System.Collections.Generic;
using Portable.Licensing;

namespace MFractor.Licensing.Issuer
{
    public class LicenseSigningConfig
    {
        public string Passphrase { get; }

        public string PublicKey { get; }

        public string PrivateKey { get; }

        public LicenseSigningConfig(string passphrase, 
                                    string publicKey, 
                                    string privateKey)
        {
            Passphrase = passphrase;
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public void Assert()
        {
            if (string.IsNullOrEmpty(PublicKey))
            {
                throw new InvalidOperationException("No public key has been specified for license signing");
            }

            if (string.IsNullOrEmpty(PrivateKey))
            {
                throw new InvalidOperationException("No private key has been specified for license signing");
            }
        }
    }
}
