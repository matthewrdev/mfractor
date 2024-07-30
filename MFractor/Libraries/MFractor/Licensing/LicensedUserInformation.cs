using System;

namespace MFractor.Licensing
{
    class LicensedUserInformation
    {
        public string Email { get; }

        public string Name { get; }

        public string Company { get; }

        public bool HasName => !string.IsNullOrEmpty(Name);

        public bool HasCompany => !string.IsNullOrEmpty(Company);

        public LicensedUserInformation(string email, string name, string company)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            Email = email;
            Name = name;
            Company = company;
        }
    }
}

