using System;
using System.Globalization;

namespace MFractor.i18n
{
    public interface ICultureProvider
    {
        CultureInfo CurrentCulture { get; }

        event EventHandler<EventArgs> OnCurrentCultureChanged;
    }
}
