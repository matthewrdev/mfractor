using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.CSharp.CodeGeneration;
using MFractor.Workspace;

namespace MFractor.Maui.CodeGeneration.ValueConversion
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IValueConversionSettings))]
    class ValueConversionSettings : Configurable, IValueConversionSettings
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public ValueConversionSettings(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator
        {
            get;
            set;
        }

        [ExportProperty("The namespace to place value conversion code inside. When empty, new value conversion code will be placed under the projects default namespace. Place a single '.' before the namespace name to make it relative to the projects default namespace.")]
        public string Namespace
        {
            get; set;
        } = ".Converters";

        [ExportProperty("The folder to place value conversion code inside. When empty, new value conversion code will be placed inside the proejcts root folder.")]
        public string Folder
        {
            get; set;
        } = "Converters";

        [ExportProperty("The namespace name of the xmlns import statement for the newly created value converter")]
        public string DefaultConverterXmlns
        {
            get; set;
        } = "converters";

        public override string Identifier => "com.mfractor.configuration.xaml.value_conversions";

        public override string Name => "Value Conversion Settings";

        public override string Documentation => "A collection of common settings to use for value converters.";

        public string CreateConvertersClrNamespace(ProjectIdentifier project, string folderPath = null)
        {
            var namespaceName = ProjectService.GetDefaultNamespace(project);

            if (!string.IsNullOrEmpty(folderPath))
            {
                namespaceName = NamespaceDeclarationGenerator.GetNamespaceFor(project, folderPath);
            }
            else if (!string.IsNullOrEmpty(Namespace))
            {
                namespaceName = Namespace.StartsWith(".", System.StringComparison.Ordinal) ? namespaceName + Namespace : Namespace;
            }

            return namespaceName;
        }

        public string GetItemFilePath(string fileName)
        {
            if (!string.IsNullOrEmpty(Folder))
            {
                return Path.Combine(Folder, fileName);
            }

            return fileName;
        }
    }
}