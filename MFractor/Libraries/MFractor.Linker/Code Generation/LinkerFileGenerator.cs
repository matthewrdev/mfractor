using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Configuration.Attributes;
using MFractor.Linker.Helpers;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Linker.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILinkerFileGenerator))]
    class LinkerFileGenerator : CodeGenerator, ILinkerFileGenerator
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        ITextProviderService TextProviderService => textProviderService.Value;

        [ExportProperty("When creating a new linker file for iOS and Android projects, what is it's name?")]
        public string DefaultLinkerFileName { get; set; } = "linker.xml";

        [ExportProperty("When creating a new linker file for iOS and Android projects, which folder should it be placed within?")]
        public string DefaultLinkerFileFolder { get; set; } = "Properties";

        public string DefaultLinkerFilePath => Path.Combine(DefaultLinkerFileFolder, DefaultLinkerFileName);

        [Import]
        public ILinkerEntryGenerator LinkerEntryGenerator { get; set; }

        public override string[] Languages { get; } = new string[] { "XML" };

        public override string Identifier => "com.mfractor.code_gen.linker.linker_file";

        public override string Name => "Linker File Generation";

        public override string Documentation => "The linker file generator can create a new linker file for an Android or iOS project. It can also add new symbols to an existing linker file";

        [ImportingConstructor]
        public LinkerFileGenerator(Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                   Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                   Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.textProviderService = textProviderService;
        }

        public IProjectFile GetLinkerFileForProject(Project project)
        {
            var linker = ProjectService.GetProjectFilesWithBuildAction(project, "LinkDescription");

            return linker.FirstOrDefault();
        }

        public IReadOnlyList<IWorkUnit> AddLinkedSymbols(Project project, IEnumerable<ISymbol> symbols)
        {
            var linkerFile = GetLinkerFileForProject(project);

            if (linkerFile == null)
            {
                return CreateLinkerFile(project, symbols);
            }

            return InsertLinkerSymbols(project, linkerFile, symbols);
        }

        public IReadOnlyList<IWorkUnit> CreateLinkerFile(Project project, IEnumerable<ISymbol> symbols)
        {
            return CreateLinkerFile(project, DefaultLinkerFilePath, symbols);
        }

        public IReadOnlyList<IWorkUnit> CreateLinkerFile(Project project, string filePath, IEnumerable<ISymbol> symbols)
        {
            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var content = CreateLinkerFileContent(symbols, policy);

            return new CreateProjectFileWorkUnit()
            {
                BuildAction = "LinkDescription",
                FilePath = filePath,
                TargetProject = project,
                InferWhenInSharedProject = false,
                ShouldOpen = true,
                AllowPostProcessing = false,
                ShouldOverWrite = true,
                FileContent = content
            }.AsList();
        }

        public string CreateLinkerFileContent(IEnumerable<ISymbol> symbols, IXmlFormattingPolicy formattingPolicy)
        {
            var data = new LinkerFileData(symbols);

            return CreateLinkerFileContent(data, formattingPolicy);
        }

        public string CreateLinkerFileContent(LinkerFileData data, IXmlFormattingPolicy formattingPolicy)
        {
            data.Deduplicate();

            var root = new XmlNode()
            {
                Name = new XmlName(LinkerKeywords.Elements.Linker),
            };

            root = BuildLinkerSyntaxTree(root, data);

            return XmlSyntaxWriter.WriteNode(root, string.Empty, formattingPolicy, true, true, true);
        }

        public IReadOnlyList<IWorkUnit> InsertLinkerSymbols(Project project, IProjectFile projectFile, IEnumerable<ISymbol> symbols)
        {
            XmlSyntaxTree syntaxTree = null;

            try
            {
                syntaxTree = XmlSyntaxParser.ParseText(TextProviderService.GetTextProvider(projectFile.FilePath));
            }
            catch (Exception) { } // Suppress parser exceptions

            if (syntaxTree == null
                || syntaxTree.Root == null
                || syntaxTree.Root.Name.FullName != LinkerKeywords.Elements.Linker)
            {
                return CreateLinkerFile(project, symbols);
            }

            var data = new LinkerFileData(symbols);
            data.Deduplicate();

            var linkerRoot = BuildLinkerSyntaxTree(syntaxTree.Root, data);

            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var content = XmlSyntaxWriter.WriteNode(linkerRoot, string.Empty, policy, true, true, true);

            return new ReplaceTextWorkUnit()
            {
                FilePath = projectFile.FilePath,
                Span = syntaxTree.Root.Span,
                Text = content
            }.AsList();
        }

        XmlNode BuildLinkerSyntaxTree(XmlNode root, LinkerFileData data)
        {
            BuildAssemblyNodes(data.Assemblies, root);

            BuildNamespaceNodes(data.Namespaces, root);

            BuildTypeNodes(data.NamedTypes, root);

            BuildMemberNodes(data.Members, root);

            return root;
        }

        void BuildMemberNodes(Dictionary<INamedTypeSymbol, List<ISymbol>> members, XmlNode root)
        {
            foreach (var pair in members)
            {
                var type = pair.Key;

                var assemblyNode = root.Children
                                   .Where(c => c.Name.FullName == LinkerKeywords.Elements.Assembly)
                                   .FirstOrDefault(c => c.GetAttributeByName(LinkerKeywords.Attributes.FullName)?.Value?.Value == type.ContainingAssembly.Name);

                if (assemblyNode != null)
                {
                    var typeNode = assemblyNode.GetChildNode(c => c.Name.FullName == LinkerKeywords.Elements.Type && c.GetAttributeByName(LinkerKeywords.Attributes.FullName)?.Value?.Value == type.ToString());

                    if (typeNode != null)
                    {
                        foreach (var member in pair.Value)
                        {

                            if (member is IFieldSymbol field && !Contains(typeNode, field, LinkerKeywords.Elements.Field))
                            {
                                typeNode.AddChildren(LinkerEntryGenerator.CreateLinkerEntry(field));
                            }
                            else if (member is IPropertySymbol property)
                            {
                                if (property.GetMethod != null && !Contains(typeNode, property.GetMethod, LinkerKeywords.Elements.Method))
                                {
                                    typeNode.AddChildren(LinkerEntryGenerator.CreateLinkerEntry(property.GetMethod));
                                }

                                if (property.SetMethod != null && !Contains(typeNode, property.SetMethod, LinkerKeywords.Elements.Method))
                                {
                                    typeNode.AddChildren(LinkerEntryGenerator.CreateLinkerEntry(property.SetMethod));
                                }
                            }
                            else if (member is IMethodSymbol method && !Contains(typeNode, method, LinkerKeywords.Elements.Method))
                            {
                                typeNode.AddChildren(LinkerEntryGenerator.CreateLinkerEntry(method));
                            }
                        }
                    }
                }
            }
        }

        bool Contains(XmlNode typeNode, ISymbol symbol, string keyword)
        {
            var symbols = typeNode.GetChildren(cn => cn.Name.FullName == keyword);

            if (symbols == null || !symbols.Any())
            {
                return false;
            }

            var signature = LinkerSignatureHelper.GetSignature(symbol);
            var name = symbol.Name;

            foreach (var s in symbols)
            {
                if (s.GetAttributeByName(LinkerKeywords.Attributes.Name)?.Value?.Value == name)
                {
                    return true;
                }

                if (s.GetAttributeByName(LinkerKeywords.Attributes.Signature)?.Value?.Value == signature)
                {
                    return true;
                }
            }

            return false;
        }

        void BuildTypeNodes(Dictionary<IAssemblySymbol, List<INamedTypeSymbol>> namedTypes, XmlNode root)
        {
            foreach (var pair in namedTypes)
            {
                var assembly = pair.Key;

                var assemblyNode = root.Children
                                   .Where(c => c.Name.FullName == LinkerKeywords.Elements.Assembly)
                                   .FirstOrDefault(c => c.GetAttributeByName(LinkerKeywords.Attributes.FullName)?.Value?.Value == assembly.Name);

                if (assemblyNode != null)
                {
                    foreach (var type in pair.Value)
                    {
                        var existing = assemblyNode.GetChildNode(c => c.Name.FullName == LinkerKeywords.Elements.Type && c.GetAttributeByName(LinkerKeywords.Attributes.FullName)?.Value?.Value == type.ToString());

                        if (existing == null)
                        {
                            assemblyNode.AddChildren(LinkerEntryGenerator.CreateLinkerEntry(type));
                        }
                    }
                }
            }
        }

        void BuildNamespaceNodes(Dictionary<IAssemblySymbol, List<INamespaceSymbol>> namespaces, XmlNode root)
        {
            foreach (var pair in namespaces)
            {
                var assembly = pair.Key;

                var assemblyNode = root.Children
                                   .Where(c => c.Name.FullName == LinkerKeywords.Elements.Assembly)
                                   .FirstOrDefault(c => c.GetAttributeByName(LinkerKeywords.Attributes.FullName)?.Value?.Value == assembly.Name);

                if (assemblyNode != null)
                {
                    foreach (var ns in pair.Value)
                    {
                        var existing = assemblyNode.GetChildNode(c => c.Name.FullName == LinkerKeywords.Elements.Namespace && c.GetAttributeByName(LinkerKeywords.Attributes.FullName)?.Value?.Value == ns.ToString());

                        if (existing == null)
                        {
                            assemblyNode.AddChildren(LinkerEntryGenerator.CreateLinkerEntry(ns));
                        }
                    }
                }
            }
        }

        void BuildAssemblyNodes(List<IAssemblySymbol> assemblies, XmlNode linkerNode)
        {
            foreach (var assembly in assemblies)
            {
                var nodes = LinkerEntryGenerator.CreateLinkerEntry(assembly);

                foreach (var assemblyNode in nodes)
                {
                    var name = assemblyNode.GetAttributeByName(LinkerKeywords.Attributes.FullName);

                    if (name != null)
                    {
                        var childNode = linkerNode.GetChildNode((cn => cn.GetAttributeByName(LinkerKeywords.Attributes.FullName)?.Value?.Value == name.Value.Value));

                        if (childNode == null)
                        {
                            linkerNode.AddChildNode(assemblyNode);
                        }
                    }
                }
            }
        }
    }
}