using MFractor.Maui.XamlPlatforms;
using MFractor.Workspace;

namespace MFractor.Maui.CodeGeneration.ValueConversion
{
    public class ValueConverterGenerationOptions
    {
        public string Name { get; set; }

        public ProjectIdentifier Project { get; set; }

        public IXamlPlatform Platform { get; set; }

        public string InputType { get; set; }

        public string OutputType { get; set; }

        public string ParameterType { get; set; }

        public string FolderPath { get; set; }

        public string Namespace { get; set; }

        public IProjectFile XamlEntryProjectFile { get; set; }
    }
}
