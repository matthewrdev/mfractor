using System;
using MFractor.Documentation;

namespace MFractor.Configuration.PropertyConverters
{
    class BooleanPropertyConverter : TypedPropertyConverter<bool>
    {
        public override string Identifier => "com.mfractor.configuration.property_converters.boolean";

        public override string Name => typeof(bool).ToString();

        public override string Documentation => "Boolean properties support inputs of `true` or `false`.";

        public override bool ApplyValue(ConfigurationId configId,
                                        IPropertySetting setting,
                                        IConfigurableProperty property,
                                        out string errorMessage)
        {
            errorMessage = "";
            
            bool b;
            var value = setting.Value.Trim();
            if (bool.TryParse(value, out b))
            {
                property.Value = b;
                return true;
            }

            errorMessage = $"The value {value} couldn't be parsed as an boolean";
            return false;
        }
    }
}
