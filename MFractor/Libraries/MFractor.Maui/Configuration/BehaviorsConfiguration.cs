using MFractor.Configuration;
using MFractor.Configuration.Attributes;

namespace MFractor.Maui.Configuration
{
    class BehaviorsConfiguration : Configurable, IBehaviorsConfiguration
    {
        public override string Name => "Behaviors Configuration";

        public override string Identifier => "com.mfractor.configuration.xaml.behaviors";

        public override string Documentation => "Groups all configuration settings related to Behavior's into a single place.";

        [ExportProperty("The default folder to place new behaviors into.")]
        public string BehaviorsFolder
        {
            get;
            set;
        } = "Behaviors";

        [ExportProperty("The default names that new behaviors will be placed into. Prepending the namespace with a `.` will cause the namespace to be appended to the projects default namespace.")]
        public string BehaviorsNamespace
        {
            get;
            set;
        } = ".Behaviors";
    }
}