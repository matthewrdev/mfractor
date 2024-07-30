using System;
namespace MFractor.Licensing.Issuer
{
    public class LicenseKeyPairGenerator
    {
        public readonly int KeySize;

        public readonly byte[] Seed;


        public LicenseKeyPairGenerator(int keySize = 256, byte[] seed = null)
        {
            KeySize = keySize;
            Seed = seed;
        }

        public LicenseSigningConfig GenerateKeyPair(string passphrase)
        {
            var keyGenerator = new Portable.Licensing.Security.Cryptography.KeyGenerator(KeySize, Seed);
            var keyPair = keyGenerator.GenerateKeyPair();
            var privateKey = keyPair.ToEncryptedPrivateKeyString(passphrase);
            var publicKey = keyPair.ToPublicKeyString();

            return new LicenseSigningConfig(passphrase, publicKey, privateKey);
        }
    }
}
