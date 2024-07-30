using System;
using Portable.Licensing.Validation;

namespace MFractor.Licensing.Consumer
{
    internal class LicenseExpiredValidation : IValidationFailure
    {
        public readonly DateTime Expiration;

        public LicenseExpiredValidation(DateTime expiration)
        {
            this.Expiration = expiration;
            _message = $"This license expired at {expiration.ToString()}";
        }

        public string HowToResolve
        {
            get;
            set;
        } = "Please provide a non-expired license key.";

        readonly string _message;
        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
            }
        }
    }
    
}
