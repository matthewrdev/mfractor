using System;
using MFractor.Documentation;

namespace MFractor.Configuration.PropertyConverters
{
    class DoublePropertyConverter : TypedPropertyConverter<double>
    {
        public override string Identifier => "com.mfractor.configuration.property_converters.double";

        public override string Name => "Double";

        public override string Documentation => "When MFractor encounters a `double` property, it will parse the value in the ";

        public override bool ApplyValue(ConfigurationId configId,
                                        IPropertySetting setting,
                                        IConfigurableProperty property,
                                        out string errorMessage)
        {
            errorMessage = "";

            double n;
            var value = setting.Value.Trim();
            if (double.TryParse(value, out n))
            {
                property.Value = n;
                return true;
            }

            errorMessage = $"The value {value} couldn't be parsed as an double";
            return false;
        }
    }
}
