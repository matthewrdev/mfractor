using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Text;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.MSBuild
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IItemGroupFinder))]
    class ItemGroupFinder : IItemGroupFinder
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ItemGroupFinder(Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                      Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.textProviderService = textProviderService;
        }

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        public IEnumerable<XmlNode> FindItemGroups(Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            return FindItemGroups(project.FilePath);
        }

        public IEnumerable<XmlNode> FindItemGroups(string projectFilePath)
        {
            if (string.IsNullOrEmpty(projectFilePath))
            {
                throw new ArgumentException("message", nameof(projectFilePath));
            }

            var textProvider = TextProviderService.GetTextProvider(projectFilePath);

            if (textProvider == null)
            {
                log?.Warning($"Failed to read the contents of {projectFilePath}");
                return Enumerable.Empty<XmlNode>();
            }

            var syntaxTree = XmlSyntaxParser.ParseText(textProvider.GetText());

            if (syntaxTree == null)
            {
                log?.Warning($"Failed to parse the MSBuild of {projectFilePath}");
                return Enumerable.Empty<XmlNode>();
            }

            return FindItemGroups(syntaxTree);
        }

        public IEnumerable<XmlNode> FindItemGroups(XmlSyntaxTree syntaxTree)
        {
            if (syntaxTree is null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            var root = syntaxTree.Root;
            if (root == null)
            {
                log?.Warning($"Cannot locate the item group as the MSBuild syntax tree does not have a root element");
                return null;
            }

            return root.GetChildren(c => c.Name.FullName == "ItemGroup");
        }
    }
}