using System;
using System.Collections.Generic;

namespace MFractor.Configuration.Mocks
{
    /// <summary>
    /// A mock property setting.
    /// </summary>
    public class MockPropertySetting : IPropertySetting
    {
        public MockPropertySetting(string configurationId,
                                   string name, string value,
                                   IConfigurationSource parent,
                                   PropertyAssignmentType assignmentType,
                                   Dictionary<string, string> metaData)
        {
            ConfigurationId = configurationId;
            Name = name;
            Value = value;
            Parent = parent;
            AssignmentType = assignmentType;
            MetaData = metaData;
        }

        /// <summary>
        /// The idententifier of the <see cref="MFractor.Configuration.IConfigurable"/> that this property setting applies to.
        /// </summary>
        /// <value>The configuration identifier.</value>
        public string ConfigurationId { get; }

        /// <summary>
        /// The name of the property that this property setting applies to.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// The new value to apply onto the <see cref="MFractor.Configuration.IConfigurable"/> property.,
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// The parent/owner of this property setting.
        /// </summary>
        /// <value>The parent.</value>
        public IConfigurationSource Parent { get; }

        /// <summary>
        /// When the configuration parser parsed this property setting,
        /// </summary>
        /// <value>The type of the assignment.</value>
        public PropertyAssignmentType AssignmentType { get; }

        /// <summary>
        /// A collection of key-value pairs that were additionally included onto the property setting.
        /// </summary>
        /// <value>The meta data.</value>
        public Dictionary<string, string> MetaData { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new <see cref="MockConfigurableProperty"/> from the <paramref name="setting"/>.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static MockPropertySetting From(IPropertySetting setting)
        {
            return new MockPropertySetting(setting.ConfigurationId,
                                           setting.Name,
                                           setting.Value,
                                           new MockConfigurationSource(setting.Parent.FilePath),
                                           setting.AssignmentType,
                                           setting.MetaData);
        }
    }
}
