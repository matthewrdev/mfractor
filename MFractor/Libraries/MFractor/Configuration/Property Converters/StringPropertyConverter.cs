using System;
using MFractor.Documentation;

namespace MFractor.Configuration.PropertyConverters
{
    class StringPropertyConverter : TypedPropertyConverter<string> 
    {
        public override string Identifier => "com.mfractor.configuration.property_converters.string";

        public override string Name => typeof(string).Name;

        public override string Documentation => "String property types are parsed and converted into a string literal.";

        public override bool ApplyValue(ConfigurationId configId, 
                                        IPropertySetting setting, 
                                        IConfigurableProperty property, 
                                        out string errorMessage)
        {
            errorMessage = "";

            property.Value = setting.Value;

            return true;
        }
    }
}
