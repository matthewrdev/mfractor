using System;
using System.Collections.Generic;

namespace MFractor.Configuration
{
    public interface IConfigurationRule : IConfigurable
    {
        int Priority { get; }

        Dictionary<string, string> Arguments { get; }

        IEnumerable<IPropertySetting> Apply(IEnumerable<IPropertySetting> properties);
    }
}
