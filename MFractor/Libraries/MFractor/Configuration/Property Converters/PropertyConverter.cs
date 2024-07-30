using System;
using System.ComponentModel.Composition;
using MFractor.Configuration.Attributes;

namespace MFractor.Configuration.PropertyConverters
{
    /// <summary>
    /// The base class for property converters.
    /// <para/>
    /// Property converters enable new data types to be exposed to MFractor configuration file engine,
    /// <para/>
    /// If you wanted to expose a property on an MFractor configurable with [ExportProperty], you should implement a property converter that understands how to create
    /// a new instance of that proper 
    /// </summary>
    [SuppressConfiguration]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IConfigurationPropertyConverter))]
    public abstract class PropertyConverter : Configurable, IConfigurationPropertyConverter
    {
        public virtual int Priority => 0;

        public virtual string Category => "General Property Types";

        public abstract bool ApplyValue(ConfigurationId configId, IPropertySetting setting, IConfigurableProperty property, out string errorMessage);
        public abstract bool Supports(Type type);
    }
}
