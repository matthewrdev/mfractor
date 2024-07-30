using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using MFractor.Logging;
using MFractor.Utilities;

namespace MFractor.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IConfigurationEngine))]
    sealed class ConfigurationEngine : IConfigurationEngine
    {
        readonly ILogger log = Logger.Create();

        readonly Lazy<IReadOnlyDictionary<string, IConfigurable>> configurablesMap;
        IReadOnlyDictionary<string, IConfigurable> ConfigurablesMap => configurablesMap.Value;

        public IEnumerable<IConfigurable> Configurables => ConfigurablesMap.Values;

        readonly Dictionary<Type, IConfigurable[]> typeCollectionCache = new Dictionary<Type, IConfigurable[]>();

        readonly Lazy<IConfigurableRepository> configurableRepository;
        IConfigurableRepository ConfigurableRepository => configurableRepository.Value;

        [ImportingConstructor]
        public ConfigurationEngine(Lazy<IConfigurableRepository> configurableRepository)
        {
            this.configurableRepository = configurableRepository;

            configurablesMap = new Lazy<IReadOnlyDictionary<string, IConfigurable>>(() =>
           {
                var results = new Dictionary<string, IConfigurable>();

                var count = 0;
                foreach (var configurable in ConfigurableRepository.Configurables)
                {
                    try
                    {
                        if (results.ContainsKey(configurable.Identifier))
                        {
                            var existing = results[configurable.Identifier];
                            log?.Warning($"The configurable cache already contains an element with the ID: {existing.Identifier}. The incoming element {configurable.ToString()} will replace {existing.ToString()}. Was this intended?");
                            Debugger.Break();
                        }

                        results[configurable.Identifier] = configurable;

                        // Give the properties a nudge so that they populate.
                        var props = configurable.Properties;

                        count++;
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }

                log?.Info("Discovered " + results.Values.Count + " configurables.");

                return results;
           });
        }

        /// <summary>
        /// Resolves an instance of the provided type.
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="type">Type.</param>
        /// <param name="id">The configuration id.</param>
        public object Resolve(Type type, ConfigurationId id = null)
        {
            if (type is null)
            {
                return default;
            }

            var configurable = GetConfigurablesOfType(type).FirstOrDefault();

            if (configurable != null)
            {
                configurable.PrepareConfigurableDependencies(this);
                configurable.ApplyConfiguration(id ?? ConfigurationId.Empty);
            }

            return configurable;
        }

        /// <summary>
        /// Resolve an instance of <typeparamref name="TConfigurable"/>.
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="id">Identifier.</param>
        /// <typeparam name="TConfigurable">The 1st type parameter.</typeparam>
        public TConfigurable Resolve<TConfigurable>(ConfigurationId id = null) where TConfigurable : class, IConfigurable
        {
            return Resolve(typeof(TConfigurable), id) as TConfigurable;
        }

        public TCastType Resolve<TConfigurable, TCastType>(ConfigurationId id = null) where TConfigurable : class, IConfigurable
                                                        where TCastType : class, TConfigurable, IConfigurable
        {
            var configurable = GetConfigurablesOfType(typeof(TConfigurable)).OfType<TCastType>()
                                                                            .FirstOrDefault();

            if (configurable != null)
            {
                configurable.PrepareConfigurableDependencies(this);
                configurable.ApplyConfiguration(id ?? ConfigurationId.Empty);
            }

            return configurable as TCastType;
        }

        public IEnumerable<TConfigurable> ResolveAll<TConfigurable>(ConfigurationId id = null) where TConfigurable : class, IConfigurable
        {
            var elements = GetConfigurablesOfType(typeof(TConfigurable));
            if (elements != null && elements.Any())
            {
                foreach (var c in elements)
                {
                    c.PrepareConfigurableDependencies(this);
                    c.ApplyConfiguration(id ?? ConfigurationId.Empty);
                }
            }

            return elements.Cast<TConfigurable>();
        }

        public bool Contains(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return false;
            }

            return ConfigurablesMap.ContainsKey(identifier);
        }

        public IConfigurable GetByIdentifier(string identifier)
        {
            if (!Contains(identifier))
            {
                return null;
            }

            return ConfigurablesMap[identifier];
        }

        public IEnumerable<TConfigurable> GetConfigurablesOfType<TConfigurable>() where TConfigurable : IConfigurable
        {
            var type = typeof(TConfigurable);
            if (typeCollectionCache.ContainsKey(type))
            {
                return typeCollectionCache[type].Cast<TConfigurable>();
            }

            var matches = Configurables.OfType<TConfigurable>().ToList();

            typeCollectionCache[type] = matches.Cast<IConfigurable>().ToArray();

            return matches;
        }

        public IEnumerable<IConfigurable> GetConfigurablesOfType(Type type)
        {
            if (type is null)
            {
                return Enumerable.Empty<IConfigurable>();
            }

            if (typeCollectionCache.ContainsKey(type))
            {
                return typeCollectionCache[type];
            }

            var matches = Configurables.Where(c => type.IsAssignableFrom(c.GetType())).ToList();

            typeCollectionCache[type] = matches.ToArray();

            return matches;
        }
    }
}
