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
    [Export(typeof(IInterfaceDeclarationGenerator))]
    class InterfaceDeclarationGenerator : CSharpCodeGenerator, IInterfaceDeclarationGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.interface_declaration";

        public override string Name => "Generate Interface Declareation";

        public override string Documentation => "A code generator that creates a new interface declaration.";

        [ExportProperty("The code snippet of a new interface declaration.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the interface")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace the interface is within.")]
        [CodeSnippetResource("Resources/Snippets/InterfaceDeclaration.txt")]
        public ICodeSnippet Snippet
        {
            get;
            set;
        }

        public IReadOnlyList<IWorkUnit> Generate(string interfaceName, string folderPath, Project project)
        {
            var code = GenerateCode(interfaceName, folderPath, project);

            var outputPath = Path.Combine(folderPath, interfaceName + ".cs");

            return new CreateProjectFileWorkUnit(code, outputPath, project.GetIdentifier()).AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(string interfaceName, string folderPath, Project project)
        {
            return SyntaxFactory.ParseCompilationUnit(GenerateCode(interfaceName, folderPath, project));
        }

        public string GenerateCode(string interfaceName, string folderPath, Project project)
        {
            var namespaceName = ProjectService.GetDefaultNamespace(project);

            if (string.IsNullOrEmpty(folderPath) == false)
            {
                var namespaceFolder = folderPath.Replace("/", ".").Replace("\\", ".").Replace(" ", "");
                namespaceName += "." + namespaceFolder;
            }

            return Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, interfaceName)
                          .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName)
                          .ToString();
        }
    }
}