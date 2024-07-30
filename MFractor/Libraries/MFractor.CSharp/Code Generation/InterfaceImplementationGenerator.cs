using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MFractor.Work;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Workspace.Utilities;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IInterfaceImplementationGenerator))]
    class InterfaceImplementationGenerator : CSharpCodeGenerator, IInterfaceImplementationGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.generate_inteface_implementation";

        public override string Name => "Interface Implemenation Code Generator";

        public override string Documentation => "A code generator that creates a C# class to implement a given interface.";

        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator
        {
            get; set;
        }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator
        {
            get; set;
        }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        public IReadOnlyList<IWorkUnit> Generate(CompilationWorkspace workspace, INamedTypeSymbol interfaceType, string folderPath, string className, Project project)
        {
            var code = GenerateCode(workspace, interfaceType, folderPath, className, project);

            var outputPath = Path.Combine(folderPath, className + ".cs");

            return new CreateProjectFileWorkUnit(code, outputPath, project.GetIdentifier()).AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(CompilationWorkspace workspace, INamedTypeSymbol interfaceType, string folderPath, string className, Project project)
        {
            var classDeclaration = ClassDeclarationGenerator.GenerateSyntax(className, interfaceType as INamedTypeSymbol, Enumerable.Empty<MemberDeclaration>());

            var namespaceName = ProjectService.GetDefaultNamespace(project);

            if (string.IsNullOrEmpty(folderPath) == false)
            {
                var namespaceFolder = folderPath.Replace("/", ".").Replace("\\", ".").Replace(" ", "");
                namespaceName += "." + namespaceFolder;
            }

            var usingSyntax = UsingDirectiveGenerator.GenerateSyntax("System");

            var namespaceSyntax = NamespaceDeclarationGenerator.GenerateSyntax(namespaceName);
            namespaceSyntax = namespaceSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDeclaration));

            var unit = SyntaxFactory.CompilationUnit().AddUsings(usingSyntax).AddMembers(namespaceSyntax);

            var options = FormattingPolicyService.GetFormattingPolicy(project);

            unit = (CompilationUnitSyntax)Microsoft.CodeAnalysis.Formatting.Formatter.Format(unit, workspace, options.OptionSet);

            return unit;
        }

        public string GenerateCode(CompilationWorkspace workspace, INamedTypeSymbol interfaceType, string folderPath, string className, Project project)
        {
            return GenerateSyntax(workspace, interfaceType, folderPath, className, project).ToString();
        }

        public IReadOnlyList<IWorkUnit> Generate(CompilationWorkspace workspace, string interfaceType, string folderPath, string className, Project project)
        {
            var code = GenerateCode(workspace, interfaceType, folderPath, className, project);

            var outputPath = Path.Combine(folderPath, className + ".cs");

            return new CreateProjectFileWorkUnit(code, outputPath, project.GetIdentifier()).AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(CompilationWorkspace workspace, string interfaceType, string folderPath, string className, Project project)
        {
            var classDeclaration = ClassDeclarationGenerator.GenerateSyntax(className, interfaceType, Enumerable.Empty<MemberDeclaration>());

            var namespaceName = ProjectService.GetDefaultNamespace(project);

            if (string.IsNullOrEmpty(folderPath) == false)
            {
                var namespaceFolder = folderPath.Replace("/", ".").Replace("\\", ".").Replace(" ", "");
                namespaceName += "." + namespaceFolder;
            }

            var usingSyntax = UsingDirectiveGenerator.GenerateSyntax("System");

            var namespaceSyntax = NamespaceDeclarationGenerator.GenerateSyntax(namespaceName);
            namespaceSyntax = namespaceSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDeclaration));

            var unit = SyntaxFactory.CompilationUnit().AddUsings(usingSyntax).AddMembers(namespaceSyntax);

            var options = FormattingPolicyService.GetFormattingPolicy(project);

            unit = (CompilationUnitSyntax)Microsoft.CodeAnalysis.Formatting.Formatter.Format(unit, workspace, options.OptionSet);

            return unit;
        }

        public string GenerateCode(CompilationWorkspace workspace, string interfaceType, string folderPath, string className, Project project)
        {
            return GenerateSyntax(workspace, interfaceType, folderPath, className, project).ToString();
        }
    }
}