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
    [Export(typeof(IEventArgsDeclarationGenerator))]
    public class EventArgsDeclarationGenerator : CSharpCodeGenerator, IEventArgsDeclarationGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.event_args_declaration";

        public override string Name => "Generate Attribute Declaration";

        public override string Documentation => "A code generator that creates a new attribute declaration.";

        [ExportProperty("The code snippet of a new interface declaration.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the event args declaration.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Namespace, "The namespace that encapsulates the event args declaration.")]
        [CodeSnippetResource("Resources/Snippets/EventArgsDeclaration.txt")]
        public ICodeSnippet Snippet
        {
            get;
            set;
        }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator
        {
            get; set;
        }

        public IReadOnlyList<IWorkUnit> Generate(string eventArgsName, string folderPath, Project project)
        {
            var code = GenerateCode(eventArgsName, folderPath, project);

            var outputPath = Path.Combine(folderPath, eventArgsName + ".cs");

            return new CreateProjectFileWorkUnit(code, outputPath, project.GetIdentifier()).AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(string eventArgsName, string folderPath, Project project)
        {
            return SyntaxFactory.ParseCompilationUnit(GenerateCode(eventArgsName, folderPath, project));
        }

        public string GenerateCode(string eventArgsName, string folderPath, Project project)
        {
            var namespaceName = NamespaceDeclarationGenerator.GetNamespaceFor(project, folderPath);

            return Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, eventArgsName)
                          .SetArgumentValue(ReservedCodeSnippetArgumentName.Namespace, namespaceName)
                          .ToString();
        }
    }
}