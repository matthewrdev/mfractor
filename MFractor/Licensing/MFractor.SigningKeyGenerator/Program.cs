using System;
using System.IO;

namespace MFractor.SigningKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            const string Passphrase = "REDACTED";

            var keyGenerator = Portable.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();
            var privateKey = keyPair.ToEncryptedPrivateKeyString(Passphrase);
            var publicKey = keyPair.ToPublicKeyString();

            File.WriteAllLines("keys.txt", new string[] { Passphrase, publicKey, privateKey });
        }
    }
}
