using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MFractor.IOC;

namespace MFractor.Configuration
{
    /// <summary>
    /// An implementation of <see cref="IPartRepository"/> that encapsulates and provides an <see cref="IConfigurable"/> implementation.
    /// </summary>
    /// <typeparam name="TConfigurable"></typeparam>
    public abstract class ConfigurablePartRepository<TConfigurable> : IConfigurablePartRepository<TConfigurable> where TConfigurable : class, IConfigurable
    {
        readonly Lazy<IConfigurationEngine> configurationEngine;
        public IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        readonly Lazy<IReadOnlyList<TConfigurable>> configurables;
        public IReadOnlyList<TConfigurable> Configurables => configurables.Value;

        public IReadOnlyList<TConfigurable> Parts => Configurables;

        public ConfigurablePartRepository(Lazy<IConfigurationEngine> configurationEngine)
        {
            this.configurationEngine = configurationEngine;
            configurables = new Lazy<IReadOnlyList<TConfigurable>>(() =>
           {
               return ConfigurationEngine.ResolveAll<TConfigurable>().ToList();
           });
        }

        public TPart GetConfigurable<TPart>(ConfigurationId configurationId = null) where TPart : TConfigurable
        {
            var result = Configurables.OfType<TPart>().FirstOrDefault();

            result?.ApplyConfiguration(configurationId);

            return result;
        }

        public TPart GetPart<TPart>() where TPart : TConfigurable
        {
            return GetConfigurable<TPart>();
        }

        public IEnumerator<TConfigurable> GetEnumerator()
        {
            return Parts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parts.GetEnumerator();
        }
    }
}
