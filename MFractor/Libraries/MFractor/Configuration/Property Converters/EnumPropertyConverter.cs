using System;
using MFractor.Documentation;

namespace MFractor.Configuration.PropertyConverters
{
    class EnumPropertyConverter : PropertyConverter
	{
        public override string Identifier => "com.mfractor.configuration.property_converters.enum";

        public override string Name => "Enum";

        public override string Documentation => "When an enum value is encountered in a confg file, MFractor will try to parse the value into the properties enum type, falling back to the properties default enum value.";

        public override bool ApplyValue(ConfigurationId configId,
                                        IPropertySetting setting,
                                        IConfigurableProperty property,
                                        out string errorMessage)
		{
            errorMessage = "";
			var value = setting.Value.Trim();

            object result = null;
            try
            {
                result = Enum.Parse(property.PropertyType, value);
            }
            catch { }

            if (result != null)
            {
                property.Value = result;
                return true;
            }

            errorMessage = $"The value {value} couldn't be parsed into an enum value of type \"{property.PropertyType}\"";

			return false;
		}

        public override bool Supports(Type type)
        {
            return type.IsEnum;
        }
    }
}
