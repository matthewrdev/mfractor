using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Composition;

namespace MFractor.Configuration
{
    /// <summary>
    /// The base class for all configurable types inside MFractor; when creating new configurable types it is recommended you inherit from this instead of <see cref="IConfigurable"/>.
    /// <para/>
    /// The <see cref="Configurable"/> base class sets up most of the infrastructure needed to support user configurations.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    public abstract class Configurable : IConfigurable
    {
        /// <summary>
        /// A unique identifier that can be used to reference this configurable. The identifier should be lower case and only contain letters, numbers, '.' and '_' characters.
        /// <para/>
        /// The identifer should be lower case and formatted as follows: 'com.product.feature_set.platform.feature_name'.
        /// <para/>
        /// - "com": This should always be the starting element.
        /// <para/>
        /// - "product": The name of the product or extension that is declaring a new configurable. For example: "mfractor" or "my_extension".
        /// <para/>
        /// - "feature_set": The feature group that this configurable lies within. For example: "code_actions" or "analysis".
        /// <para/>
        /// - (Optional) "platform":  The platform or technology that this configurable targets. For example: "wpf", "android" or "csharp".
        /// <para/>
        /// - "feature_name": A short, descriptive name of this configurable, similiar to its <see cref="P:MFractor.Documentation.IAmDocumented.Name" />.
        /// </summary>
        /// <value>The identifier.</value>
        public abstract string Identifier
        {
            get;
        }

        /// <summary>
        /// A short (3-6) word description of this element.
        /// <para/>
        /// This is the title of the documentation section of this element.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// A complete overview of the element and what it does; something like a "mini-tutorial".
        /// <para/>
        /// This documentation will be exposed in the final auto-generated documentation.
        /// </summary>
        /// <value>The documentation.</value>
        public abstract string Documentation
        {
            get;
        }

        ConfigurablePropertyContainer container;
        ConfigurablePropertyContainer Container
        {
            get
            {
                if (container == null)
                {
                    container = new ConfigurablePropertyContainer(this);
                }

                return container;
            }
        }

        /// <summary>
        /// The configurable properties available for this configurable.
        /// </summary>
        /// <value>The properties.</value>
        public IReadOnlyList<IConfigurableProperty> Properties => Container.Properties;

        /// <summary>
        /// A list of other configurables that this configurable uses.
        /// </summary>
        /// <value>The dependencies.</value>
        public IReadOnlyList<IConfigurable> Dependencies => Container.Dependencies;

        /// <summary>
        /// Finds the property named <paramref name="name" /> and returns the configurable
        /// </summary>
        /// <returns>The configurable property or null.</returns>
        /// <param name="name">The property name.</param>
        public IConfigurableProperty GetConfigurableProperty(string name)
        {
            return Container.GetConfigurableProperty(name);
        }

        /// <summary>
        /// Prepares the properties for this configurable using the configuration id suppplied.
        /// <para/>
        /// This method MUST be invoked for a users project settings to take effect on a configurable
        /// before any other methods are invoked.
        /// <para/>
        /// If this <see cref="MFractor.Configuration.IConfigurable"/> has the <see cref="MFractor.Configuration.Attributes.SuppressConfigurationAttribute"/> applied, configuration will be ignored.
        /// </summary>
        /// <param name="configId">The configuration identifier</param>
        public void ApplyConfiguration(ConfigurationId configId)
        {
            Container.ApplyConfiguration(configId);
        }

        /// <summary>
        /// Resolve all other <see cref="IConfigurable"/> instances that this configurable uses.
        /// </summary>
        /// <param name="engine">The configuration engine.</param>
        public void PrepareConfigurableDependencies(IConfigurationEngine engine)
        {
            Container.PrepareConfigurableDependencies(engine);
        }

        /// <summary>
        /// Applies the configuration <paramref name="configId" /> onto <paramref name="property" />.
        /// <para/>
        /// You can use this to dynamically change the configuration state of an IConfigurable if you need to target the users
        /// configuration settings for another project or solution.
        /// </summary>
        /// <param name="property">The name of the property to apply the configuration onto.</param>
        /// <param name="configId">The configuration identifier</param>
        public void ApplyPropertyConfiguration(string property, ConfigurationId configId)
        {
            Container.ApplyPropertyConfiguration(property, configId);
        }
    }
}
