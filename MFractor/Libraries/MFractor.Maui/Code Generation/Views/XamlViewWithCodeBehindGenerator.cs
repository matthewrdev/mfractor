using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.BindableProperties;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Work;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Maui.CodeGeneration.Views
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlViewWithCodeBehindGenerator))]
    class XamlViewWithCodeBehindGenerator : CodeGenerator, IXamlViewWithCodeBehindGenerator
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        public override string[] Languages { get; } = new string[] { "XAML", "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.xaml_view_with_code_behind";

        public override string Name => "Generate View With XAML and Code-Behind Class";

        public override string Documentation => "Creates a new view/control using a XAML to define the UI and a code-behind class to encapsulate backing logic.";

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        [Import]
        public IBindablePropertyGenerator BindablePropertyGenerator { get; set; }

        [ImportingConstructor]
        public XamlViewWithCodeBehindGenerator(Lazy<IXmlSyntaxWriter> xmlSyntaxWriter)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
        }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new class.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the new class resides inside.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The base type of the new class.")]
        [CodeSnippetArgument("xmlns", "The xmlns to use when the view base class is not a platform base class.")]
        [CodeSnippetResource("Resources/Snippets/View.xaml.txt")]
        [ExportProperty("What is the default implementation of the new XAML control's XAML file?")]
        public ICodeSnippet ViewSnippet { get; set; }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new class.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the new class resides inside.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The base type of the new class.")]
        [CodeSnippetResource("Resources/Snippets/CodeBehindClass.txt")]
        [ExportProperty("What is the default implementation of the new XAML control's code behind class?")]
        public ICodeSnippet CodeBehindSnippet { get; set; }

        public IReadOnlyList<IWorkUnit> Generate(string className,
                                               string namespaceName,
                                               string namespaceXmlnsName,
                                               Project project,
                                               IXamlPlatform platform,
                                               string folder,
                                               string baseClass)
        {
            return Generate(className, namespaceName, namespaceXmlnsName, project.GetIdentifier(), project.Solution.Workspace, platform, folder, baseClass);
        }

        public IReadOnlyList<IWorkUnit> Generate(string className,
                                               string namespaceName,
                                               string namespaceXmlnsName,
                                               ProjectIdentifier projectIndentifier,
                                               CompilationWorkspace workspace,
                                               IXamlPlatform platform,
                                               string folder,
                                               string baseClass)
        {
            var options = FormattingPolicyService.GetFormattingPolicy(projectIndentifier);

            var compilation = ProjectService.GetCompilation(projectIndentifier);

            var xaml = GenerateXAMLView(className, namespaceName, baseClass, namespaceXmlnsName, platform, compilation);

            var codeBehindMetaType = baseClass;

            var unit = GenerateCodeBehind(className,
                                          codeBehindMetaType,
                                          namespaceName,
                                          workspace,
                                          options.OptionSet);

            unit = (CompilationUnitSyntax)Formatter.Format(unit, workspace, options.OptionSet);

            var dotNetCode = unit.ToString();

            var xamlFileName = className + ".xaml";
            var codeBehindFileName = className + ".xaml.cs";

            if (!string.IsNullOrEmpty(folder))
            {
                xamlFileName = Path.Combine(folder, xamlFileName);
                codeBehindFileName = Path.Combine(folder, codeBehindFileName);
            }

            return new List<IWorkUnit>()
            {
                new CreateProjectFileWorkUnit
                {
                    BuildAction = "Compile",
                    FilePath = codeBehindFileName,
                    DependsUponFile = xamlFileName,
                    FileContent = dotNetCode,
                    TargetProjectIdentifier = projectIndentifier
                },
                new CreateProjectFileWorkUnit
                {
                    BuildAction = "EmbeddedResource",
                    ResourceId = projectIndentifier.Name + "." + xamlFileName.Replace("/", ".").Replace("\\", "."),
                    FilePath = xamlFileName,
                    FileContent = xaml,
                    TargetProjectIdentifier = projectIndentifier,
                    Generator = "MSBuild:UpdateDesignTimeXaml"
                }
            };
        }

        public IReadOnlyList<IWorkUnit> Generate(string className,
                                               string classNamespace,
                                               Project project,
                                               string folder,
                                               IXmlFormattingPolicy xmlPolicy,
                                               XmlNode rootNode,
                                               IXamlPlatform platform,
                                               IXamlNamespaceCollection namespaces,
                                               INamedTypeSymbol codeBehindType)
        {
            if (project is null)
            {
                return Array.Empty<IWorkUnit>();
            }

            return Generate(className, classNamespace, project.GetIdentifier(), project.Solution.Workspace, folder, xmlPolicy, rootNode, platform, namespaces, codeBehindType);
        }

        public IReadOnlyList<IWorkUnit> Generate(string className,
                                               string classNamespace,
                                               ProjectIdentifier projectIdentifier,
                                               CompilationWorkspace workspace,
                                               string folder,
                                               IXmlFormattingPolicy xmlPolicy,
                                               XmlNode rootNode,
                                               IXamlPlatform platform,
                                               IXamlNamespaceCollection namespaces,
                                               INamedTypeSymbol codeBehindType)
        {
            var policy = new OverloadableXmlFormattingPolicy(xmlPolicy)
            {
                MaxAttributesPerLine = 1
            };

            var options = FormattingPolicyService.GetFormattingPolicy(projectIdentifier);

            var xaml = GenerateXAMLView(className, classNamespace, rootNode, platform, namespaces);

            var codeBehindMetaType = codeBehindType == null ? platform.View.MetaType : codeBehindType.ToString();

            var xmlCode = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xmlPolicy.NewLineChars + XmlSyntaxWriter.WriteNode(xaml, "", policy, true, true, false);

            var unit = GenerateCodeBehind(className,
                                          codeBehindMetaType,
                                          classNamespace,
                                          workspace,
                                          options.OptionSet);

            unit = (CompilationUnitSyntax)Formatter.Format(unit, workspace, options.OptionSet);

            var dotNetCode = unit.ToString();

            var xamlFileName = className + ".xaml";
            var codeBehindFileName = className + ".xaml.cs";

            if (!string.IsNullOrEmpty(folder))
            {
                xamlFileName = Path.Combine(folder, xamlFileName);
                codeBehindFileName = Path.Combine(folder, codeBehindFileName);
            }

            return new List<IWorkUnit>()
            {
                new CreateProjectFileWorkUnit
                {
                    BuildAction = "Compile",
                    FilePath = codeBehindFileName,
                    DependsUponFile = xamlFileName,
                    FileContent = dotNetCode,
                    TargetProjectIdentifier = projectIdentifier
                },
                new CreateProjectFileWorkUnit
                {
                    BuildAction = "EmbeddedResource",
                    ResourceId = projectIdentifier.Name + "." + xamlFileName.Replace("/", ".").Replace("\\", "."),
                    FilePath = xamlFileName,
                    FileContent = xmlCode,
                    TargetProjectIdentifier = projectIdentifier,
                    Generator = "MSBuild:UpdateDesignTimeXaml"
                },
            };
        }

        public CompilationUnitSyntax GenerateCodeBehind(string className,
                                                        string baseType,
                                                        string namespaceName,
                                                        CompilationWorkspace workspace,
                                                        OptionSet formattingOptions)
        {
            var unit = CodeBehindSnippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, className)
                                        .SetArgumentValue(ReservedCodeSnippetArgumentName.Type, baseType)
                                        .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName)
                                        .AsCompilationUnit();

            unit = Formatter.Format(unit, workspace, formattingOptions) as CompilationUnitSyntax;

            return unit;

        }


        public string GenerateXAMLView(string className,
                                       string namespaceName,
                                       string baseClass,
                                       string xmlnsPrefix,
                                       IXamlPlatform platform,
                                       Compilation compilation)
        {
            var xmlns = $"xmlns=\"{platform.SchemaUrl}\"";

            var baseClassName = baseClass.Split('.').Last();

            if (compilation != null)
            {
                var symbol = compilation.GetTypeByMetadataName(baseClass);

                if (symbol == null)
                {
                    xmlns = CreateXmlns(baseClass, xmlnsPrefix, xmlns);
                }
                else if (symbol.ContainingAssembly.Name != platform.Assembly)
                {
                    xmlns += "\n " + XamlNamespaceImportGenerator.GenerateXmlnsImportStatement(xmlnsPrefix, symbol, symbol.ContainingAssembly.Name != compilation.AssemblyName);

                    if (!string.IsNullOrEmpty(xmlnsPrefix))
                    {
                        baseClassName = xmlnsPrefix + ":" + baseClassName;
                    }
                }
            }
            else
            {
                CreateXmlnsAndBaseClass(baseClass, xmlnsPrefix, ref xmlns, ref baseClassName);
            }

            return ViewSnippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, className)
                              .SetArgumentValue(ReservedCodeSnippetArgumentName.Type, baseClassName)
                              .SetArgumentValue("xmlns", xmlns)
                              .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName)
                              .ToString();
        }

        void CreateXmlnsAndBaseClass(string baseClass, string xmlnsPrefix, ref string xmlns, ref string baseClassName)
        {
            if (!string.IsNullOrEmpty(xmlnsPrefix))
            {
                baseClassName = xmlnsPrefix + ":" + baseClassName;
            }

            xmlns = CreateXmlns(baseClass, xmlnsPrefix, xmlns);
        }

        string CreateXmlns(string baseClass, string xmlnsPrefix, string xmlns)
        {
            var components = baseClass.Split('.');

            var xmlnsValue = "";

            if (components.Length > 1)
            {
                xmlnsValue = string.Join(".", components.Take(components.Length - 1));
                xmlns += " " + XamlNamespaceImportGenerator.GenerateXmlnsImportStatement(xmlnsPrefix, xmlnsValue, string.Empty, false);
            }

            return xmlns;
        }

        public XmlNode GenerateXAMLView(string className,
                                        string namespaceName,
                                        XmlNode rootNode,
                                        IXamlPlatform platform,
                                        IXamlNamespaceCollection namespaces)
        {
            var root = new XmlNode
            {
                IsSelfClosing = false,
                Name = new XmlName("View")
            };

            if (rootNode != null)
            {
                root = rootNode.Clone();
            }

            root.AddAttribute("xmlns", platform.SchemaUrl);
            root.AddAttribute("xmlns:x", XamlSchemas.MicrosoftSchemaUrl);
            root.AddAttribute("x:Class", namespaceName + "." + className);

            foreach (var xmlns in namespaces.Namespaces)
            {
                if (xmlns.Schema != null)
                {
                    continue;
                }

                if (!root.HasAttribute(a => a.Name.HasNamespace
                                        && a.Name.Namespace == "xmlns"
                                        && a.Name.LocalName == xmlns.Prefix))

                {
                    root.AddAttribute("xmlns:" + xmlns.Prefix, xmlns.Value);
                }
            }

            return root;
        }
    }
}
