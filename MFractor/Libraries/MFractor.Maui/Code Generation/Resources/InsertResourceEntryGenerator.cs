using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Utilities;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Resources
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IInsertResourceEntryGenerator))]
    class InsertResourceEntryGenerator : CodeGenerator, IInsertResourceEntryGenerator
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public override string[] Languages { get; } = new string[] { "xaml" };

        public override string Identifier => "com.mfractor.code_generation.xaml.insert_resource_entry";

        public override string Name => "Insert Resource Entry";

        public override string Documentation => "Inserts a resource dictionary entry into a page, view or app xaml";

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<ITextProviderService> textProviderService = new Lazy<ITextProviderService>();
        ITextProviderService TextProviderService => textProviderService.Value;

        [ImportingConstructor]
        public InsertResourceEntryGenerator(Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                            Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.textProviderService = textProviderService;
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project,
                                               string filePath,
                                               XmlSyntaxTree xmlSyntaxTree,
                                               XmlNode node)
        {
            if (xmlSyntaxTree == null || xmlSyntaxTree.Root == null || node == null)
            {
                log?.Warning("Cannot insert element into the resources as the target XML syntax tree is null");
                return null;
            }

            var insertionLocation = InsertionLocation.Start;
            XmlSyntax anchor = default;

            var root = xmlSyntaxTree.Root;

            var resourcesSetter = root.GetChildNode(c =>
            {
                if (c.Name.FullName == root.Name.FullName + ".Resources")
                {
                    return true;
                }

                return false;
            });

            XmlNode host = null;
            XmlNode insertion = null;

            if (resourcesSetter != null)
            {
                var resourceDictionary = resourcesSetter.GetChildNode(c =>
                {
                    if (c.Name.FullName == "ResourceDictionary")
                    {
                        return true;
                    }

                    return false;
                });

                if (resourceDictionary != null)
                {
                    host = resourceDictionary;
                    insertion = node;
                    anchor = resourceDictionary.GetChildren(c => XamlSyntaxHelper.IsPropertySetter(c)).LastOrDefault();
                    if (anchor != null)
                    {
                        insertionLocation = InsertionLocation.End;
                    }
                }
                else
                {
                    host = resourcesSetter;
                    insertion = node;
                }
            }
            else
            {
                if (root.Name.FullName != "ResourceDictionary")
                {
                    var resources = new XmlNode("ResourceDictionary");
                    resources.AddChildNode(node);

                    var setter = new XmlNode(root.Name.FullName + ".Resources");
                    setter.AddChildNode(resources);

                    host = root;
                    insertion = setter;
                }
                else
                {
                    host = root;
                    insertion = node;
                }
            }

            return new InsertXmlSyntaxWorkUnit()
            {
                FilePath = filePath,
                HostSyntax = host,
                Syntax = insertion,
                AnchorSyntax = anchor,
                InsertionLocation = insertionLocation
            }.AsList();
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, string filePath, XmlNode node)
        {
            if (!File.Exists(filePath))
            {
                return Array.Empty<IWorkUnit>();
            }

            var textProvider = TextProviderService.GetTextProvider(filePath);

            var xmlSyntaxTree = XmlSyntaxParser.ParseText(textProvider.GetText());

            return Generate(project, filePath, xmlSyntaxTree, node);
        }

        public IReadOnlyList<IWorkUnit> Generate(IProjectFile projectFile, XmlSyntaxTree xmlSyntaxTree, XmlNode node)
        {
            return Generate(projectFile.CompilationProject, projectFile.FilePath, xmlSyntaxTree, node);
        }

        public IReadOnlyList<IWorkUnit> Generate(IProjectFile projectFile, XmlNode node)
        {
            return Generate(projectFile.CompilationProject, projectFile.FilePath, node);
        }
    }
}