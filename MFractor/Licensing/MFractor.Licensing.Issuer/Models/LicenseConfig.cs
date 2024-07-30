using System;
using System.Collections.Generic;
using Portable.Licensing;

namespace MFractor.Licensing.Issuer
{
    public class LicenseConfig
    {
        public Guid Guid { get; }

        public string Name { get; }

        public string Email { get; }

        public string ProductSku { get; }

        public DateTime IssuedAtUtc { get; }

        public TimeSpan LicenseDuration { get; }

        public bool IsTrial { get; }

        public int MaxUtilisations { get; }

        public Dictionary<string, string> MetaData { get; }

        public DateTime Expiry
        {
            get
            {
                return IssuedAtUtc + LicenseDuration;
            }
        }

        public LicenseConfig(Guid guid,
                             string name,
                             string email,
                             string productSku,
                             DateTime issuedAtUtc,
                             TimeSpan licenseDuration,
                             bool isTrial,
                             int maxUtilisations,
                             Dictionary<string, string> metaData)
        {
            MaxUtilisations = maxUtilisations;
            Guid = guid;
            Name = name;
            Email = email;
            ProductSku = productSku;
            IssuedAtUtc = issuedAtUtc;
            LicenseDuration = licenseDuration;
            IsTrial = isTrial;
            MetaData = metaData;
        }

        public void Assert()
        {
            if (Guid == null)
            {
                throw new InvalidOperationException($"No {nameof(Guid)} has been set for this license");
            }

            if (string.IsNullOrEmpty(Name))
            {
                throw new InvalidOperationException($"No {nameof(Name)} has been set for this license");
            }

            if (string.IsNullOrEmpty(Email))
            {
                throw new InvalidOperationException($"No {nameof(Email)} has been set for this license");
            }

            if (string.IsNullOrEmpty(ProductSku))
            {
                throw new InvalidOperationException($"No {nameof(ProductSku)} has been set for this license");
            }
        }
    }

}