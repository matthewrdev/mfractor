using System.Collections.Generic;

namespace MFractor.Configuration
{
    public interface IPropertySetting
    {
        /// <summary>
        /// The idententifier of the <see cref="IConfigurable"/> that this property setting applies to.
        /// </summary>
        /// <value>The configuration identifier.</value>
        string ConfigurationId { get; }

        /// <summary>
        /// The name of the property that this property setting applies to.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// The new value to apply onto the <see cref="IConfigurable"/> property.,
        /// </summary>
        /// <value>The value.</value>
        string Value { get; }

        /// <summary>
        /// The parent/owner of this property setting.
        /// </summary>
        /// <value>The parent.</value>
        IConfigurationSource Parent { get; }

        /// <summary>
        /// When the configuration parser parsed this property setting, 
        /// </summary>
        /// <value>The type of the assignment.</value>
        PropertyAssignmentType AssignmentType { get; }

        /// <summary>
        /// A collection of key-value pairs that were additionally included onto the property setting.
        /// </summary>
        /// <value>The meta data.</value>
        Dictionary<string, string> MetaData { get; }
    }
}
