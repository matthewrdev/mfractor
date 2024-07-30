using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Portable.Licensing;
using Portable.Licensing.Validation;

namespace MFractor.Licensing.Consumer
{

    internal class InvalidProductValidation : IValidationFailure
    {
        public string HowToResolve
        {
            get;
            set;
        } = "Please use a license that is for this product";

        public string Message
        {
            get;
            set;
        } = "The license is not associated with this product";
    }
    
}
