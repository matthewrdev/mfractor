using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAttributeDeclarationGenerator))]
    class AttributeDeclarationGenerator : CSharpCodeGenerator, IAttributeDeclarationGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.attribute_declaration";

        public override string Name => "Generate Attribute Declaration";

        public override string Documentation => "A code generator that creates a new attribute declaration.";

        [ExportProperty("The code snippet of a new interface declaration.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the attribute")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace the interface is within.")]
        [CodeSnippetResource("Resources/Snippets/AttributeDeclaration.txt")]
        public ICodeSnippet Snippet { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        public IReadOnlyList<IWorkUnit> Generate(string attributeName, string folderPath, Project project)
        {
            var code = GenerateCode(attributeName, folderPath, project);

            var outputPath = Path.Combine(folderPath, attributeName + ".cs");

            return new CreateProjectFileWorkUnit(code, outputPath, project.GetIdentifier()).AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(string attributeName, string folderPath, Project project)
        {
            return SyntaxFactory.ParseCompilationUnit(GenerateCode(attributeName, folderPath, project));
        }

        public string GenerateCode(string attributeName, string folderPath, Project project)
        {
            var namespaceName = NamespaceDeclarationGenerator.GetNamespaceFor(project, folderPath);

            return Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, attributeName)
                          .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName)
                          .ToString();
        }
    }
}