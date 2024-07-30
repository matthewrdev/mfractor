using System;
using System.Collections.Generic;

namespace MFractor.Maui.FontSizes
{
    public interface IFontSizeConfigurationService
    {
        IReadOnlyDictionary<string, IFontSize> FontSizes { get; }

        IFontSize GetNamedFontSize(string name);

        bool TryGetNamedFontSize(string name, out IFontSize fontSize);
    }
}
