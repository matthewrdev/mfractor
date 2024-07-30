using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDefaultResourceFile))]
    class DefaultResourceFile : Configurable, IDefaultResourceFile
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        public override string Identifier => "com.mfractor.configuration.resx.default_resource_file";

        public override string Name => "Default Resource File";

        public override string Documentation => "A proxy configuration that allows code actions and code generators throughout MFractor to use the same default resource file";

		[ExportProperty("What is the default resource file that string resources should be placed into?")]
		public string ProjectFilePath { get; set; } = "Resources/Resources.resx";


        [ImportingConstructor]
        public DefaultResourceFile(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        public string GetFullyQualifiedDefaultResourceSymbolName(Project project)
        {
            var projectNamespace = ProjectService.GetDefaultNamespace(project);

            var symbolName = projectNamespace + "." + ProjectFilePath.Replace("\\", ".").Replace("/", ".").Replace(".resx", "");

            return symbolName;
        }
    }
}
