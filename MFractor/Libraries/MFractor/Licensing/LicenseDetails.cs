using System;

namespace MFractor.Licensing
{
    public class LicenseDetails
    {
        public string Name { get; }

        public string Email { get; }

        public bool IsPaid { get; }

        public bool IsTrial { get; }

        public bool IsActivated { get; }

        public bool HasExpired { get; }

        public DateTime? Expiry { get; }

        public LicenseDetails(string name,
                              string email,
                              bool isPaid,
                              bool isTrial,
                              bool isActivated,
                              bool hasExpired,
                              DateTime? expiry)
        {
            Name = name;
            Email = email;
            IsPaid = isPaid;
            IsTrial = isTrial;
            IsActivated = isActivated;
            HasExpired = hasExpired;
            Expiry = expiry;
        }

        public bool HasName => string.IsNullOrEmpty(Name) == false;

        public bool HasEmail => string.IsNullOrEmpty(Email) == false;

        public LicenseKind Type
        {
            get
            {
                if (IsTrial)
                {
                    return LicenseKind.Trial;
                }

                if (IsPaid)
                {

                    return LicenseKind.Professional;
                }

                return LicenseKind.Lite;
            }
        }


        public string Description
        {
            get
            {
                if (IsTrial)
                {
                    return $"Trial License";
                }

                if (IsPaid)
                {
                    return $"Professional License";
                }

                if (IsActivated)
                {
                    return "Lite License";
                }

                return "Inactive License";
            }
        }
    }

}