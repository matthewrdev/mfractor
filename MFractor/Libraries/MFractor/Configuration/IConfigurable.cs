using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Documentation;

namespace MFractor.Configuration
{
    [InheritedExport]
    public interface IConfigurable : IAmDocumented
    {
        /// <summary>
        /// A unique identifier that can be used to reference this configurable. The identifier should be lower case and only contain letters, numbers, '.' and '_' characters.
        /// <para/>
        /// The identifer should, ideally, be formatted as follows: 'com.product.feature_set.platform.feature_name'.
        /// <para/>
        ///  - "com": This should always be the starting element.
        /// <para/>
        ///  - "product": The name of the product or extension that is declaring a new configurable. For example: "mfractor" or "my_extension".
        /// <para/>
        ///  - "feature_set": The feature group that this configurable lies within. For example: "code_actions" or "analysis".
        /// <para/>
        ///  - (Optional) "platform":  The platform or technology that this configurable targets. For example: "wpf", "android" or "csharp".
        /// <para/>
        ///  - "feature_name": A short, descriptive name of this configurable, similiar to its <see cref="IAmDocumented.Name"/>.
        /// </summary>
        /// <value>The identifier.</value>
        string Identifier { get; }

        /// <summary>
        /// The configurable properties available for this configurable.
        /// </summary>
        /// <value>The properties.</value>
        IReadOnlyList<IConfigurableProperty> Properties { get; }

        /// <summary>
        /// A list of other configurables that this configurable uses.
        /// </summary>
        /// <value>The dependencies.</value>
        IReadOnlyList<IConfigurable> Dependencies { get; }

        /// <summary>
        /// Finds the property named <paramref name="name"/> and returns the configurable 
        /// </summary>
        /// <returns>The configurable property or null.</returns>
        /// <param name="name">The property name.</param>
        IConfigurableProperty GetConfigurableProperty(string name);

        /// <summary>
        /// Prepares the properties for this configurable using the configuration id suppplied.
        /// <para/>
        /// This method MUST be invoked for a users project settings to take effect on a configurable before any other methods are invoked.
        /// <para/>
        /// If this <see cref="IConfigurable"/> has the <see cref="MFractor.Configuration.Attributes.SuppressConfigurationAttribute"/> applied, configuration will be ignored.
        /// </summary>
        /// <param name="configId">The configuration identifier</param>
        void ApplyConfiguration(ConfigurationId configId);

        /// <summary>
        /// Applies the configuration <paramref name="configId"/> onto <paramref name="property"/>.
        /// <para/>
        /// You can use this to dynamically change the configuration state of an IConfigurable if you need to target the users configuration settings for another project or solution.
        /// </summary>
        /// <param name="property">The name of the property to apply the configuration onto.</param>
        /// <param name="configId">The configuration identifier</param>
        void ApplyPropertyConfiguration(string property, ConfigurationId configId);

        /// <summary>
        /// Resolve all other <see cref="IConfigurable"/> instances that this configurable uses.
        /// </summary>
        /// <param name="engine">The configuration engine.</param>
        void PrepareConfigurableDependencies(IConfigurationEngine engine);
    }
}
