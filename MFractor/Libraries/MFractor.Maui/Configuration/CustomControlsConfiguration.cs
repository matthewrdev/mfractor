using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;

namespace MFractor.Maui.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICustomControlsConfiguration))]
    class CustomControlsConfiguration : Configurable, ICustomControlsConfiguration
    {
        public override string Name => "Custom Controls Configuration";

        public override string Identifier => "com.mfractor.configuration.xaml.custom_controls";

        public override string Documentation => "Groups all configuration settings related to custom controls into a single place.";

        [ExportProperty("What is the folder that new controls should be placed into?")]
        public string ControlsFolder { get; set; } = "Controls";

        [ExportProperty("What is the default namespace the new controls be placed into?")]
        public string ControlsNamespace { get; set; } = ".Controls";
    }
}