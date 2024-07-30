using System;
using System.ComponentModel.Composition;

namespace MFractor.CSharp
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDateTimeFormatValidator))]
    class DateTimeFormatValidator : IDateTimeFormatValidator
    {
        public bool IsValidDateTimeFormat(string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return false;
            }

            try
            {
                var formattedDate = DateTime.UtcNow.ToString(format);
                DateTime.Parse(formattedDate);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}