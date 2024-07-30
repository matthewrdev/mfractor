using System;

namespace MFractor.CSharp
{
    public interface IDateTimeFormatValidator
    {
        bool IsValidDateTimeFormat(string format);
    }
}