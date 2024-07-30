using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Code.Documents;
using MFractor.Maui.XamlPlatforms;
using MFractor.Text;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.CodeGeneration.Xaml
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlNamespaceImportGenerator))]
    class XamlNamespaceImportGenerator : CodeGenerator, IXamlNamespaceImportGenerator
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        public override string[] Languages { get; } = new string[] { "XAML" };

        public override string Identifier => "com.mfractor.code_gen.xaml.xmlns_generator";

        public override string Name => "Xml Namespace Generator";

        public override string Documentation => "Generates a xmlns statement to import a namespace and assembly";

        [ImportingConstructor]
        public XamlNamespaceImportGenerator(Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                            Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                            Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.textProviderService = textProviderService;
        }

        public string GenerateXmlnsImportStatement(string prefix, ITypeSymbol symbol, bool includeAssembly)
        {
            return $"xmlns:{prefix}=\"{GenerateXmlnsImportAttibuteValue(symbol.ContainingNamespace, includeAssembly)}\"";
        }

        public string GenerateXmlnsImportStatement(string prefix, string namespaceName, string assemblyName, bool includeAssembly)
        {
            return $"xmlns:{prefix}=\"{GenerateXmlnsImportAttibuteValue(namespaceName, assemblyName, includeAssembly)}\"";
        }

        public XmlAttribute GenerateXmlnsImportAttibute(string prefix, string namespaceValue)
        {
            var attribute = new XmlAttribute
            {
                Name = new XmlName("xmlns", prefix),
                Value = new XmlAttributeValue(GenerateXmlnsImportAttibuteValue(namespaceValue, string.Empty, false))
            };

            return attribute;
        }

        public XmlAttribute GenerateXmlnsImportAttibute(string prefix, ITypeSymbol symbol, bool includeAssembly)
        {
            var attribute = new XmlAttribute
            {
                Name = new XmlName("xmlns", prefix),
                Value = new XmlAttributeValue(GenerateXmlnsImportAttibuteValue(symbol.ContainingNamespace, includeAssembly))
            };

            return attribute;
        }

        public XmlAttribute GenerateXmlnsImportAttibute(string prefix, string namespaceName, string assemblyName, bool includeAssembly)
        {
            var attribute = new XmlAttribute
            {
                Name = new XmlName("xmlns", prefix),
                Value = new XmlAttributeValue(GenerateXmlnsImportAttibuteValue(namespaceName, assemblyName, includeAssembly))
            };

            return attribute;
        }

        public string GenerateXmlnsImportAttibuteValue(INamespaceSymbol namespaceSymbol, bool includeAssembly)
        {
            return GenerateXmlnsImportAttibuteValue(namespaceSymbol.ToString(), namespaceSymbol.ContainingAssembly.Name, includeAssembly);
        }

        public string GenerateXmlnsImportAttibuteValue(string namespaceName, string assemblyName, bool includeAssembly)
        {
            var import = $"clr-namespace:{namespaceName}";

            if (includeAssembly)
            {
                import += $";assembly={assemblyName}";
            }

            return import;
        }

        public XmlAttribute GenerateXmlnsImportAttibute(string prefix, INamespaceSymbol namespaceSymbol, bool includeAssembly)
        {
            var attribute = new XmlAttribute
            {
                Name = new XmlName("xmlns", prefix),
                Value = new XmlAttributeValue(GenerateXmlnsImportAttibuteValue(namespaceSymbol, includeAssembly))
            };

            return attribute;
        }

        public XmlAttribute GenerateXmlnsUriImportAttibute(string xmlns, string name, string uriContent)
        {
            var attribute = new XmlAttribute
            {
                Name = new XmlName("xmlns", name),
                Value = new XmlAttributeValue(uriContent)
            };

            return attribute;
        }

        public IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IProjectFile projectFile,
                                                                         IXamlPlatform platform,
                                                                         string xmlNamespaceName,
                                                                         string namespaceValue,
                                                                         IXmlFormattingPolicy policy)
        {
            var provider = TextProviderService.GetTextProvider(projectFile.FilePath);

            if (provider == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var ast = XmlSyntaxParser.ParseText(provider.GetText());
            return CreateXmlnsImportStatementWorkUnit(ast, platform, projectFile.FilePath, xmlNamespaceName, namespaceValue, policy);
        }


        public IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IProjectFile projectFile,
                                                                         IXamlPlatform platform,
                                                                         string xmlNamespaceName,
                                                                         ITypeSymbol symbol,
                                                                         Project project,
                                                                         IXmlFormattingPolicy policy)
        {
            var provider = TextProviderService.GetTextProvider(projectFile.FilePath);

            if (provider == null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var ast = XmlSyntaxParser.ParseText(provider.GetText());

            return CreateXmlnsImportStatementWorkUnit(ast, projectFile.FilePath, xmlNamespaceName, symbol, project, policy);
        }

        public IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IParsedXmlDocument document,
                                                                         IXamlPlatform platform,
                                                                         string xmlNamespaceName,
                                                                         ITypeSymbol symbol,
                                                                         Project project,
                                                                         IXmlFormattingPolicy policy)
        {
            return CreateXmlnsImportStatementWorkUnit(document.GetSyntaxTree(), document.FilePath, xmlNamespaceName, symbol, project, policy);
        }

        public IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(XmlSyntaxTree xmlSyntaxTree,
                                                                         string filePath,
                                                                         string xmlNamespaceName,
                                                                         ITypeSymbol symbol,
                                                                         Project project,
                                                                         IXmlFormattingPolicy policy)
        {
            var root = xmlSyntaxTree.Root;

            var workUnits = new List<IWorkUnit>();

            var includeAssemblyImport = project.AssemblyName != symbol.ContainingAssembly.Name;

            var importAttribute = GenerateXmlnsImportAttibute(xmlNamespaceName, symbol, includeAssemblyImport);

            var importCode = " " + XmlSyntaxWriter.WriteAttribute(importAttribute, policy);

            var import = new ReplaceTextWorkUnit()
            {
                FilePath = filePath,
                Text = importCode,
                Span = new TextSpan(root.OpeningTagSpan.End - 1, 0),
            };

            workUnits.Add(import);

            return workUnits;
        }

        public IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(IParsedXmlDocument document,
                                                                         IXamlPlatform platform,
                                                                         string xmlNamespaceName,
                                                                         string namespaceValue,
                                                                         IXmlFormattingPolicy policy)
        {
            return CreateXmlnsImportStatementWorkUnit(document.GetSyntaxTree(), platform, document.FilePath, xmlNamespaceName, namespaceValue, policy);
        }

        public IReadOnlyList<IWorkUnit> CreateXmlnsImportStatementWorkUnit(XmlSyntaxTree xmlSyntaxTree,
                                                                         IXamlPlatform platform,
                                                                         string filePath,
                                                                         string xmlNamespaceName,
                                                                         string namespaceValue,
                                                                         IXmlFormattingPolicy policy)
        {
            var root = xmlSyntaxTree.Root;

            var workUnits = new List<IWorkUnit>();

            if (!root.HasAttribute("xmlns"))
            {
                var attr = GenerateXmlnsUriImportAttibute(null, "xmlns", platform.SchemaUrl);
                var code = " " + XmlSyntaxWriter.WriteAttribute(attr, policy);

                workUnits.Add(new ReplaceTextWorkUnit()
                {
                    FilePath = filePath,
                    Text = code,
                    Span = new TextSpan(root.OpeningTagSpan.End - 1, 0),
                });
            }

            var importAttribute = GenerateXmlnsImportAttibute(xmlNamespaceName, namespaceValue);

            var importCode = " " + XmlSyntaxWriter.WriteAttribute(importAttribute, policy);

            var import = new ReplaceTextWorkUnit()
            {
                FilePath = filePath,
                Text = importCode,
                Span = new TextSpan(root.OpeningTagSpan.End - 1, 0),
            };

            workUnits.Add(import);

            return workUnits;
        }
    }
}

