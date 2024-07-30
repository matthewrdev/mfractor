using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Android.Data.Repositories.Manifest;
using MFractor.Data;
using MFractor.Workspace.Data.Synchronisation;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Workspace.Data;
using MFractor.Workspace;
using MFractor.Android.Data.Models.Manifest;

namespace MFractor.Android.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class AndroidManifestSynchroniser : ITextResourceSynchroniser
    {
        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        public string[] SupportedFileExtensions { get; } = new string[] { ".xml", };

        [ImportingConstructor]
        public AndroidManifestSynchroniser(Lazy<IXmlSyntaxParser> xmlSyntaxParser)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
        }

        public Task<bool> CanSynchronise(Solution solution, Project project, IProjectFile projectFile)
        {
            var canSynchronise = projectFile.Name == "AndroidManifest.xml";

            return Task.FromResult(canSynchronise);
        }

        public bool IsAvailable(Solution solution, Project project)
        {
            return project.IsAndroidProject();
        }

        public Task<bool> Synchronise(Solution solution,
                                Project project,
                                IProjectFile projectFile,
                                ITextProvider textProvider,
                                IProjectResourcesDatabase database)
        {
            var syntaxTree = XmlSyntaxParser.ParseText(textProvider.GetText());

            if (syntaxTree == null
                || syntaxTree.Root == null)
            {
                return Task.FromResult(false);
            }

            var packageId = syntaxTree.Root.GetAttributeByName("package")?.Value?.Value;
            var repo = database.GetRepository<PackageDetailsRepository>();

            var details = new PackageDetails();
            details.PackageName = packageId;
            repo.Insert(details);

            return Task.FromResult(false);
        }
    }
}
