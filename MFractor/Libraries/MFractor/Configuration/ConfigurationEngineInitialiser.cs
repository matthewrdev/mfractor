using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;

namespace MFractor.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ConfigurationEngineInitialiser : IApplicationLifecycleHandler
    {
        readonly Lazy<IConfigurationEngine> configurationEngine;
        public IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        [ImportingConstructor]
        public ConfigurationEngineInitialiser(Lazy<IConfigurationEngine> configurationEngine)
        {
            this.configurationEngine = configurationEngine;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            foreach (var p in ConfigurationEngine.Configurables)
            {
                p.PrepareConfigurableDependencies(ConfigurationEngine);
                p.ApplyConfiguration(ConfigurationId.Empty);
            }
        }
    }
}
