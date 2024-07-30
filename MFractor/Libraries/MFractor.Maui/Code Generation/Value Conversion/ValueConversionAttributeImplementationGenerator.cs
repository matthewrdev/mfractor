using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.ValueConversion
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IValueConversionAttributeImplementationGenerator))]
    class ValueConversionAttributeImplementationGenerator : CodeGenerator, IValueConversionAttributeImplementationGenerator
    {
		public override string[] Languages { get; } = new string[] { "C#" };

        public override string Documentation => "Generates an implementation of the `ValueConversionAttribute`; the attribute used to hint type-flow in a value converter for design time tools like MFractor.";

        public override string Identifier => "com.mfractor.code_gen.xaml.csharp.value_conversion_attribute";

        public override string Name => "Generate Value Conversion Attribute";

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that the new value conversion attribute should be placed within.")]
        [CodeSnippetResource("Resources/Snippets/ValueConversionAttribute.txt")]
        [ExportProperty("What is the default code snippet to use when creating the value conversion attribute implementation?")]
        public ICodeSnippet Snippet { get; set; }

        [Import]
        public IValueConversionSettings ValueConversionSettings { get; set; }

        public string GenerateCode(ProjectIdentifier project)
        {
            var unit = GenerateSyntax(project);

            return unit.ToString();
        }

        public IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier project)
        {
            var code = GenerateCode(project);

            var filePath =  ValueConversionSettings.GetItemFilePath("ValueConversionAttribute.cs");

            return new CreateProjectFileWorkUnit()
            {
                FilePath = filePath,
                FileContent = code,
                InferWhenInSharedProject = true,
                TargetProjectIdentifier = project,
            }.AsList();
        }

        public IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier project, string filePath)
        {
            var code = GenerateCode(project);

            return new CreateProjectFileWorkUnit()
            {
                FilePath = filePath,
                FileContent = code,
                InferWhenInSharedProject = true,
                TargetProjectIdentifier = project,
            }.AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(ProjectIdentifier project)
        {
            var namespaceName = ValueConversionSettings.CreateConvertersClrNamespace(project, null);

            Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName);

            return Snippet.AsCompilationUnit();
        }
    }
}
