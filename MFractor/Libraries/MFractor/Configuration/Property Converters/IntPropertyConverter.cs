using System;
using MFractor.Documentation;

namespace MFractor.Configuration.PropertyConverters
{
    class IntPropertyConverter : TypedPropertyConverter<int>
    {
        public override string Identifier => "com.mfractor.configuration.property_converters.integer";

        public override string Name => typeof(int).ToString();

        public override string Documentation => "When an integer value is encountered in a config file, MFractor will try to parse the value into a signed `int`. If the number fails to parse, MFractor falls back to the propeties default `integer` value.";

        public override bool ApplyValue(ConfigurationId configId,
                                        IPropertySetting setting,
                                        IConfigurableProperty property,
                                        out string errorMessage)
        {
            errorMessage = "";

            int n;
            var value = setting.Value.Trim();
            if (int.TryParse(value, out n))
            {
                property.Value = n;
                return true;
            }

            errorMessage = $"The value {value} couldn't be parsed as an integer";
            return false;
        }
    }
}
