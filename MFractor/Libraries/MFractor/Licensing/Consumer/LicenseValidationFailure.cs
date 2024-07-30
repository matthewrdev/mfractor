using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Portable.Licensing;
using Portable.Licensing.Validation;

namespace MFractor.Licensing.Consumer
{
    internal class LicenseValidationFailure : Exception
    {
        readonly System.Collections.Generic.IEnumerable<IValidationFailure> validationFailures;

        public LicenseValidationFailure(System.Collections.Generic.IEnumerable<IValidationFailure> failures)
        {
            this.validationFailures = failures;
        }

        public override string Message
        {
            get
            {
                return "Invalid license";
            }
        }

        public override string ToString()
        {
            string failures = string.Join("\n", validationFailures.Select(vf => vf.Message + " | " + vf.HowToResolve));

            return string.Format("License Validation Failure: {0}\n{1}]", Message, failures);
        }
    }
    
}
