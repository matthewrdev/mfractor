using System;
using System.Collections.Generic;

namespace MFractor.Configuration.Mocks
{
    /// <summary>
    /// A mock configuration source.
    /// </summary>
    public class MockConfigurationSource : IConfigurationSource
    {
        public MockConfigurationSource(string filePath)
        {
            FilePath = filePath;
        }

        public string Name => "Mock";

        public ConfigurationId Id => ConfigurationId.Create("mock", "mock");

        public ConfigurationId ParentId => null;

        public string FilePath { get; }

        public ConfigurationScope Scope => ConfigurationScope.AdHoc;

        public Dictionary<string, List<IPropertySetting>> Settings { get; }
    }
}
