using System;
using System.ComponentModel.Composition;
using System.Web;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Xml;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.MSBuild
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProjectFileEntryFinder))]
    class ProjectFileEntryFinder : IProjectFileEntryFinder
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ProjectFileEntryFinder(Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                      Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.textProviderService = textProviderService;
        }

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        public XmlNode FindProjectFile(Project project, IProjectFile projectFile)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (projectFile is null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            return FindProjectFile(project.FilePath, projectFile);
        }

        public XmlNode FindProjectFile(string projectFilePath, IProjectFile projectFile)
        {
            if (string.IsNullOrEmpty(projectFilePath))
            {
                throw new ArgumentException("message", nameof(projectFilePath));
            }

            if (projectFile is null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            var textProvider = TextProviderService.GetTextProvider(projectFile.FilePath);

            if (textProvider == null)
            {
                log?.Warning($"Failed to read the contents of {projectFilePath}");
                return null;
            }

            var syntaxTree = XmlSyntaxParser.ParseText(textProvider.GetText());

            if (syntaxTree == null)
            {
                log?.Warning($"Failed to parse the MSBuild of {projectFilePath}");
                return null;
            }

            return FindProjectFile(syntaxTree, projectFile);
        }

        public XmlNode FindProjectFile(XmlSyntaxTree syntaxTree, IProjectFile projectFile)
        {
            if (syntaxTree is null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            if (projectFile is null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            var root = syntaxTree.Root;
            if (root == null)
            {
                log?.Warning($"Cannot locate {projectFile.VirtualPath} as the MSBuild syntax tree does not have a root element");
                return null;
            }

            var virtualPath = PathHelper.CorrectDirectorySeparatorsInPath(projectFile.VirtualPath);

            var itemGroups = root.GetChildren(c => c.Name.FullName == "ItemGroup");

            foreach (var itemGroup in itemGroups)
            {
                var projectFileEntry = itemGroup.GetChildNode(c =>
                {
                    var include = c.GetAttributeByName("Include");

                    if (include == null || !include.HasValue)
                    {
                        return false;
                    }

                    var decodedValue = HttpUtility.UrlDecode(include.Value.Value);
                    var includePath = decodedValue.Replace("\\\\", "\\"); // Unescape paths.
                    includePath = PathHelper.CorrectDirectorySeparatorsInPath(includePath);

                    if (includePath != virtualPath)
                    {
                        return false;
                    }

                    return true;
                });

                if (projectFileEntry != null)
                {
                    return projectFileEntry;
                }
            }

            return null;
        }
    }
}