using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.IOC;
using MFractor.Utilities;

namespace MFractor.Configuration
{
    /// <summary>
    /// A container for collecting and storing available configurable properties on an <see cref="IConfigurable"/> implementation.
    /// <para/>
    /// When implementing a new <see cref="IConfigurable"/>, you can route all methods and properties into the matching ones for this class.
    /// </summary>
    public sealed class ConfigurablePropertyContainer
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly IConfigurable owner;
        readonly bool isSupressed;

        readonly Lazy<IConfigurationRepository> configurationRepository = new Lazy<IConfigurationRepository>(() => Resolver.Resolve<IConfigurationRepository>());
        IConfigurationRepository ConfigurationRepository => configurationRepository.Value;

        readonly Lazy<IConfigurationEngine> configurationEngine = new Lazy<IConfigurationEngine>(() => Resolver.Resolve<IConfigurationEngine>());
        IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        readonly Lazy<ICodeSnippetFactory> codeSnippetFactory = new Lazy<ICodeSnippetFactory>(() => Resolver.Resolve<ICodeSnippetFactory>());
        ICodeSnippetFactory CodeSnippetFactory => codeSnippetFactory.Value;

        readonly Type codeSnippetType = typeof(ICodeSnippet);

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Configuration.ConfigurablePropertyContainer"/> class.
        /// </summary>
        /// <param name="owner">Owner.</param>
        public ConfigurablePropertyContainer(IConfigurable owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));

            isSupressed = AttributeHelper.HasAttribute<SuppressConfigurationAttribute>(owner.GetType());
        }

        void TryPrepareProperties()
        {
            if (configurableProperties == null)
            {
                PrepareProperties();
                ApplyConfiguration(ConfigurationId.Empty);
            }
        }

        /// <summary>
        /// Prepare the dependencies for this 
        /// </summary>
        /// <param name="engine">Engine.</param>
        /// <param name="force">If set to <c>true</c> force.</param>
        public void PrepareConfigurableDependencies(IConfigurationEngine engine, bool force = false)
        {
            if (Dependencies == null || force)
            {
                var dependencies = ConfigurationDependencyHelper.ResolveDependencies(owner, engine);

                Dependencies = dependencies != null ? dependencies.ToArray() : Array.Empty<IConfigurable>();

                foreach (var dependency in Dependencies)
                {
                    dependency.PrepareConfigurableDependencies(engine);
                }
            }
        }

        void PrepareProperties()
        {
            var properties = new List<IConfigurableProperty>();

            if (!isSupressed)
            {
                var type = owner.GetType();

                var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

                if (props != null && props.Any())
                {
                    var exportedProps = props.Where(p => p.GetCustomAttribute<ExportPropertyAttribute>() != null).ToList();
                    if (exportedProps != null && exportedProps.Any())
                    {
                        foreach (var ep in exportedProps)
                        {
                            ConfigurableProperty prop = null;
                            try
                            {
                                prop = new ConfigurableProperty(owner, ep, ep.GetCustomAttribute<ExportPropertyAttribute>().Description);
                            }
                            catch (Exception ex)
                            {
                                log?.Error($"Failed to create a configurable property for {ep.Name}. Reason: ");
                                log?.Exception(ex);
                            }
                            if (prop != null)
                            {
                                properties.Add(prop);
                            }
                        }
                    }
                }
            }

            configurableProperties = properties.ToArray();
            propertiesDictionary = properties.ToDictionary(p => p.Name, p => p);
        }

        IReadOnlyDictionary<string, IConfigurableProperty> propertiesDictionary;
        IConfigurableProperty[] configurableProperties;

        /// <summary>
        /// The <see cref="IConfigurableProperty"/>'s available to this <see cref="ConfigurablePropertyContainer"/>.
        /// </summary>
        /// <value>The properties.</value>
        public IReadOnlyList<IConfigurableProperty> Properties
        {
            get
            {
                TryPrepareProperties();

                return configurableProperties;
            }
        }

        /// <summary>
        /// The dependencies of this <see cref="ConfigurablePropertyContainer"/>.
        /// </summary>
        /// <value>The dependencies.</value>
        public IReadOnlyList<IConfigurable> Dependencies
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the configurable property with the <paramref name="name"/>.
        /// </summary>
        /// <returns>The configurable property.</returns>
        /// <param name="name">Name.</param>
        public IConfigurableProperty GetConfigurableProperty(string name)
        {
            TryPrepareProperties();

            if (propertiesDictionary.ContainsKey(name) == false)
            {
                return null;
            }

            return propertiesDictionary[name];
        }

        /// <summary>
        /// Apply the configuration for <paramref name="configId"/>.
        /// </summary>
        /// <param name="configId">Config identifier.</param>
        public void ApplyConfiguration(ConfigurationId configId)
        {
            if (isSupressed)
            {
                return;
            }

            ResetPropertyValues();

            if (configId != null)
            {
                var properties = ConfigurationRepository.GetConfiguration(configId, owner.Identifier);

                if (properties != null
                    && properties.Any())
                {
                    foreach (var prop in properties)
                    {
                        try
                        {
                            ApplyPropertyConfiguration(prop, configId);
                        }
                        catch (Exception ex)
                        {
                            log?.Warning("An error occured while trying to set the configurable property: " + prop.Name);
                            log?.Exception(ex);
                        }
                    }
                }

                if (Dependencies != null && Dependencies.Any())
                {
                    foreach (var d in Dependencies)
                    {
                        try
                        {
                            d.ApplyConfiguration(configId);
                        }
                        catch (Exception ex)
                        {
                            log?.Exception(ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply the configuration for <paramref name="configId"/> to the property named '<paramref name="propertyName"/>'.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="configId">Config identifier.</param>
        public void ApplyPropertyConfiguration(string propertyName, ConfigurationId configId)
        {
            if (isSupressed || configId == null)
            {
                return;
            }

            var property = ConfigurationRepository.GetPropertyConfiguration(propertyName, configId, owner.Identifier);

            if (property != null)
            {
                ApplyPropertyConfiguration(property, configId);
            }
        }

        /// <summary>
        /// Apply the configuration for <paramref name="configId"/> to the <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="configId">Config identifier.</param>
        public void ApplyPropertyConfiguration(IPropertySetting property, ConfigurationId configId)
        {
            if (isSupressed || property == null)
            {
                return;
            }

            if (configId != null)
            {
                var configurableProp = GetConfigurableProperty(property.Name);

                if (configurableProp != null)
                {
                    var propertyType = configurableProp.PropertyType;
                    var converter = ConfigurationEngine.GetConfigurablesOfType< IConfigurationPropertyConverter>()
                                                        .Where(pc => pc.Supports(propertyType))
                                                        .OrderByDescending(pc => pc.Priority)
                                                        .FirstOrDefault();

                    if (converter != null)
                    {
                        var success = converter.ApplyValue(configId, property, configurableProp, out var error);

                        if (!success)
                        {
                            log?.Warning(error);
                        }
                    }
                    else
                    {
                        configurableProp.Value = property.Value;
                    }
                }
            }
            else
            {
                ApplyPropertyDefault(property.Name);
            }
        }

        void ResetPropertyValues()
        {
            if (isSupressed)
            {
                return;
            }

            foreach (var p in Properties)
            {
                try
                {
                    ApplyPropertyDefault(p);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        /// <summary>
        /// Apply the default value to the property named '<paramref name="propertyName"/>'.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        public void ApplyPropertyDefault(string propertyName)
        {
            if (isSupressed)
            {
                return;
            }

            var property = GetConfigurableProperty(propertyName);

            if (property == null)
            {
                return;
            }

            ApplyPropertyDefault(property);
        }

        /// <summary>
        /// Apply the default value to the <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        public void ApplyPropertyDefault(IConfigurableProperty property)
        {
            if (isSupressed)
            {
                return;
            }

            if (codeSnippetType.IsAssignableFrom(property.PropertyType) && property.DefaultValue == null)
            {
                property.Value = CodeSnippetFactory.GetDefaultSnippetForProperty(this.owner, property.Name);
                return;
            }

            property.Value = property.DefaultValue;
        }
    }
}
