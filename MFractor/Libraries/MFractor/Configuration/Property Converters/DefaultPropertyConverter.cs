using System;
using MFractor.Documentation;


namespace MFractor.Configuration.PropertyConverters
{
    
    class DefaultPropertyConverter : PropertyConverter
    {
        public override int Priority => int.MinValue;

        public override string Identifier => "com.mfractor.configuration.property_converters.default";

        public override string Name => "Default Value";

        public override string Documentation => "Attempts to convert the provided configuration property value into a string.";

        public override bool ApplyValue(ConfigurationId configId, 
                                        IPropertySetting setting, 
                                        IConfigurableProperty property, 
                                        out string errorMessage)
        {
            errorMessage = "";

            property.Value = setting.Value;

            return true;
        }

        public override bool Supports(Type type)
        {
            return true;
        }
    }
}
