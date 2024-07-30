using System.Collections.Generic;

namespace MFractor.Configuration
{
    public interface IConfigurationSource
    {
        string Name { get; }
        
        ConfigurationId Id { get; }

        ConfigurationId ParentId { get; }

        string FilePath { get; }

        ConfigurationScope Scope { get; }

        Dictionary<string, List<IPropertySetting>> Settings { get; }
    }
}
