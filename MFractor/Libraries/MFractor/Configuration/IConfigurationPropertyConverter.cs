using System;
using MFractor.Documentation;

namespace MFractor.Configuration
{
    public interface IConfigurationPropertyConverter : IConfigurable
    {
        int Priority { get; }

        string Category { get; }

        bool Supports(Type type);

        bool ApplyValue(ConfigurationId configId,
                        IPropertySetting setting, 
                        IConfigurableProperty property,
                        out string errorMessage);
    }
}
