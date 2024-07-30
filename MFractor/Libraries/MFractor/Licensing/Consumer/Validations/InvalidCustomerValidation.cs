using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Portable.Licensing;
using Portable.Licensing.Validation;

namespace MFractor.Licensing.Consumer
{
    class InvalidCustomerValidation : IValidationFailure
    {
        public string HowToResolve
        {
            get;
            set;
        } = "Please use the email address associated with this license";

        public string Message
        {
            get;
            set;
        } = "The license is not associated with the provided email address.";
    }
}
