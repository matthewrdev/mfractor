using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MFractor.Utilities;
using Portable.Licensing;
using Portable.Licensing.Validation;

namespace MFractor.Licensing.Consumer
{
    class PublicKey
    {
        public readonly byte[] Key;

        public PublicKey(byte[] key)
        {
            Key = key;
        }
    }

    class LicenseConsumer
    {
        License license;
        readonly PublicKey publicKey;
        readonly PublicKey legacyKey;

        public string Email => license?.Customer?.Email;

        public string Name => license?.Customer?.Name;

        public string Company => license?.Customer?.Company;

        public bool IsProfessional
        {
            get
            {
                if (license == null)
                {
                    return false;
                }

                return license.Type == LicenseType.Standard || license.Type == LicenseType.Trial;
            }
        }

        public bool IsTrial
        {
            get
            {
                if (license == null)
                {
                    return false;
                }

                return license.Type == LicenseType.Trial;
            }
        }

        public bool IsLoaded => license != null;

        public DateTime? Expiry => license?.Expiration;

        public bool HasExpired(DateTime utcNow)
        {
            if (license == null)
            {
                return true;
            }

            return utcNow > license.Expiration;
        }

        public LicenseConsumer(PublicKey publicKey, PublicKey legacyKey)
        {
            this.publicKey = publicKey;
            this.legacyKey = legacyKey;
        }

        public void Load(FileInfo licenseFile)
        {
            using (var stream = File.OpenRead(licenseFile.FullName))
            {
                Load(stream);
            }
        }

        Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void Load(string licenseContent)
        {
            var bytes = System.Convert.FromBase64String(licenseContent);
            var xml = System.Text.Encoding.UTF8.GetString(bytes);

            using (var s = GenerateStreamFromString(xml))
            {
                Load(s);
            }
        }

        public void Load(Stream licenseStream)
        {
            license = License.Load(licenseStream);
        }

        public void Unload()
        {
            license = null;
        }

        public IReadOnlyList<IValidationFailure> Validate()
        {
            if (license == null)
            {
                throw new InvalidOperationException("Cannot validate the license as no license is loaded");
            }

            return license.Validate()
                    .Signature(Encoding.UTF8.GetString(publicKey.Key))
                   .AssertValidLicense()
                   .ToList();
        }

        public IReadOnlyList<IValidationFailure> ValidateLegacy()
        {
            if (license == null)
            {
                throw new InvalidOperationException("Cannot validate the license as no license is loaded");
            }

            return license.Validate()
                    .Signature(Encoding.UTF8.GetString(legacyKey.Key))
                   .AssertValidLicense()
                   .ToList();
        }

        public bool IsValid()
        {
            if (!IsLoaded)
            {
                return false;
            }

            return !Validate().Any();
        }
    }
}