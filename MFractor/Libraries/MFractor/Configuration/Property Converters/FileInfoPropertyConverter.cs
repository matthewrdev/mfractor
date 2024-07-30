using System;
using System.IO;
using MFractor.Documentation;

namespace MFractor.Configuration.PropertyConverters
{
    class FileInfoPropertyConverter : TypedPropertyConverter<FileInfo>
    {
        public override string Identifier => "com.mfractor.configuration.property_converters.file_info";

        public override string Name => "File Path";

        public override string Documentation => "File Paths refer to files either absolutely or relative to the current project.";

        public override bool ApplyValue(ConfigurationId configId,
                                        IPropertySetting setting,
                                        IConfigurableProperty property,
                                        out string errorMessage)
        {
            errorMessage = "";

            var value = setting.Value;
            var parent = setting.Parent;
            var parentPath = new FileInfo(parent.FilePath);

            string filePath = "";

            if (Path.IsPathRooted(value))
            {
                filePath = value;
            }
            else
            {
                filePath = Path.Combine(parentPath.Directory.FullName, value);
            }

            property.Value = new FileInfo(filePath);

            return true;
        }
    }
}
