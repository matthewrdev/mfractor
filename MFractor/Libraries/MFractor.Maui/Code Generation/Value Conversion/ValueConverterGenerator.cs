using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Maui.CodeGeneration.Resources;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.ValueConversion
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IValueConverterGenerator))]
    class ValueConverterGenerator : CodeGenerator, IValueConverterGenerator
    {
        const string valueConversionAttributeName = "ValueConversionAttribute";

        [ExportProperty("If an implementation of the ValueConversionAttribute class cannot be resolved within the project compilation, should MFractor automatically create the implementation?")]
        public bool CreateMissingValueConversionAttribute { get; set; } = true;

        [Import]
        public IValueConversionAttributeImplementationGenerator ValueConversionAttributeImplementationGenerator { get; set; }

        [Import]
        public IValueConversionSettings ValueConversionSettings { get; set; }

        [Import]
        public IInsertResourceEntryGenerator InsertResourceEntryGenerator { get; set; }

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        [Import]
        public IXamlNamespaceParser XamlNamespaceResolver { get; set; }

        [Import]
        public IXmlSyntaxParser XmlSyntaxParser { get; set; }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new value converter.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace the new converter will be placed inside.")]
        [CodeSnippetArgument("value_conversion_attribute", "The fully qualified type of the value conversion attribute (if available).")]
        [CodeSnippetArgument("input_type", "The fully qualified type that the value converter should accept as it's input type. If unavailable, defaults to `System.Object`.")]
        [CodeSnippetArgument("output_type", "The fully qualified type that the value converter should accept as it's output type. If unavailable, defaults to `System.Object`.")]
        [CodeSnippetArgument("parameter_type", "The fully qualified type that the value converter should accept as it's parameter type. If unavailable, defaults to `System.Object`.")]
        [CodeSnippetArgument("value_converter_type", "The fully qualified type of the XAML platforms IValueConverter interface.")]
        [CodeSnippetResource("Resources/Snippets/ValueConverter.txt")]
        [ExportProperty("What is the code snippet to use when generating the value converter class file?")]
        public ICodeSnippet Snippet { get; set; }

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.csharp.value_converter";

        public override string Name => "Generate IValueConverter Implementation";

        public override string Documentation => "Generates an implementation of a value converter.";

        public string GenerateCode(ValueConverterGenerationOptions options)
        {
            return GenerateCode(options.Name, options.Project, options.Platform, options.Namespace, options.InputType, options.OutputType, options.ParameterType);
        }

        public string GenerateCode(string name, ProjectIdentifier project, IXamlPlatform platform, string @namespace, string inputType = "object", string outputType = "object", string parameterType = null)
        {
            var unit = GenerateSyntax(name, project, platform, @namespace, inputType, outputType, parameterType);

            return unit.ToString();
        }

        public IReadOnlyList<IWorkUnit> Generate(ValueConverterGenerationOptions options)
        {
            return Generate(options.Name, options.Project, options.Platform, options.FolderPath, options.Namespace, options.InputType, options.OutputType, options.ParameterType, options.XamlEntryProjectFile);
        }

        public IReadOnlyList<IWorkUnit> Generate(string name,
                                               ProjectIdentifier project,
                                               IXamlPlatform platform,
                                               string folderPath,
                                               string @namespace,
                                               string inputType = "object",
                                               string outputType = "object",
                                               string parameterType = "object",
                                               IProjectFile xamlEntryFile = null)
        {
            var code = GenerateCode(name, project, platform, @namespace, inputType, outputType, parameterType);

            var workUnits = new List<IWorkUnit>();

            // Do we need to create the value conversion attribute?
            if (CreateMissingValueConversionAttribute)
            {
                var compilation = ProjectService.GetCompilation(project);
                var symbol = SymbolHelper.ResolveSymbolInCompilation(valueConversionAttributeName, compilation);

                if (symbol == null)
                {
                    workUnits.AddRange(ValueConversionAttributeImplementationGenerator.Generate(project));
                }
            }

            var fileName = name + ".cs";
            var filePath = ValueConversionSettings.GetItemFilePath(fileName);

            if (!string.IsNullOrEmpty(folderPath))
            {
                filePath = Path.Combine(folderPath, fileName);
            }

            workUnits.Add(new CreateProjectFileWorkUnit(code, filePath, project));

            if (xamlEntryFile != null)
            {
                var entry = new XmlNode()
                {
                    Name = new XmlName("converters", name),
                    IsSelfClosing = true,
                };
                entry.AddAttribute(new XmlAttribute("x:Key", name.FirstCharToLower()));

                var requiresXmlnsImport = true;
                var syntaxTree = XmlSyntaxParser.ParseFile(xamlEntryFile.FileInfo);
                if (syntaxTree != null)
                {
                    var compilationProject = ProjectService.GetProject(project);
                    if (compilationProject.TryGetCompilation(out var compilation))
                    {
                        var namespaces = XamlNamespaceResolver.ParseNamespaces(syntaxTree);

                        requiresXmlnsImport = namespaces.ResolveNamespace("converters") == null;
                    }
                }

                if (requiresXmlnsImport)
                {
                    workUnits.AddRange(XamlNamespaceImportGenerator.CreateXmlnsImportStatementWorkUnit(xamlEntryFile, platform, "converters", @namespace, XmlFormattingPolicyService.GetXmlFormattingPolicy()));
                }

                workUnits.AddRange(InsertResourceEntryGenerator.Generate(xamlEntryFile.CompilationProject, xamlEntryFile.FilePath, entry));
            }

            return workUnits;
        }

        public CompilationUnitSyntax GenerateSyntax(ValueConverterGenerationOptions options)
        {
            return GenerateSyntax(options.Name,
                                  options.Project,
                                  options.Platform,
                                  options.Namespace,
                                  options.InputType,
                                  options.OutputType,
                                  options.ParameterType);
        }

        public CompilationUnitSyntax GenerateSyntax(string name,
                                                    ProjectIdentifier project,
                                                    IXamlPlatform platform,
                                                    string @namespace,
                                                    string inputType = "object",
                                                    string outputType = "object",
                                                    string parameterType = "object")
        {
            var compilation = ProjectService.GetCompilation(project);

            var valueConversionAttribute = SymbolHelper.ResolveSymbolInCompilation(valueConversionAttributeName, compilation);

            var symbolName = "ValueConversion";
            if (valueConversionAttribute != null)
            {
                symbolName = valueConversionAttribute.ContainingNamespace.ToString() + ".ValueConversion";
            }

            Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, name)
                   .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, @namespace)
                   .SetArgumentValue("value_converter_type", platform.ValueConverter.MetaType)
                   .SetArgumentValue("input_type", string.IsNullOrEmpty(inputType) ? "object" : inputType)
                   .SetArgumentValue("output_type", string.IsNullOrEmpty(outputType) ? "object" : outputType)
                   .SetArgumentValue("parameter_type", string.IsNullOrEmpty(parameterType) ? "object" : parameterType)
                   .SetArgumentValue("value_conversion_attribute", symbolName);

            return Snippet.AsCompilationUnit();
        }

        public string GetConverterTypeName(ValueConverterGenerationOptions options)
        {
            return GetConverterTypeName(options.Name, options.Project, options.FolderPath, options.Namespace);
        }

        public string GetConverterTypeName(string name, ProjectIdentifier project, string folderPath, string @namespace)
        {
            var fullNamespace = ValueConversionSettings.CreateConvertersClrNamespace(project, folderPath);

            if (!string.IsNullOrEmpty(@namespace))
            {
                fullNamespace = @namespace;
            }

            return fullNamespace + "." + name;
        }
    }
}
