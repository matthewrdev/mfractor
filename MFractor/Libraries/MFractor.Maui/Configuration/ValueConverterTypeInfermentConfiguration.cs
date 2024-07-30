using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;

namespace MFractor.Maui.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IValueConverterTypeInfermentConfiguration))]
    class ValueConverterTypeInfermentConfiguration : Configurable, IValueConverterTypeInfermentConfiguration
    {
        public override string Name => "Value Converter Type Inferment Configuration";

        public override string Identifier => "com.mfractor.configuration.xaml.value_converter_type_inferment";

        public override string Documentation => "Groups all configuration settings related to MFractors automatic type inferment based on names.";

        [ExportProperty("Should MFractor attempt to guess the type based on a name and value? EG: A value of `true` would cause the output type to be `bool` instead of `System.Object`.")]
        public bool TryInferUnknownTypes { get; set; } = true;

        [ExportProperty("When attempting to infer the the type based on a name and value and MFractor decides it's a color, what is the color type MFractor should use?")]
        public string DefaultColorType { get; set; }

        [ExportProperty("What is the default type if MFractor can't figure it out based on the attribute value?")]
        public string DefaultType { get; set; } = "System.Object";

        [ExportProperty("When attempting to infer the type based on a name and value and MFractor decides it's an image, what is the CLR type MFractor should use?")]
        public string DefaultImageType { get; set; }
    }
}