using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Code.Documents;
using MFractor.CSharp.CodeGeneration;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.CSharp.CodeActions
{
    public class GenerateInterfaceImplementation : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp.generate_interface_implementation";

        public override string Name => "Generate Interface Implementation";

        public override string Documentation => "Generates a new implementation of the given interface.";

        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        public override bool CanExecute(SyntaxNode syntax,
                                        IParsedCSharpDocument document,
                                        IFeatureContext context,
                                        InteractionLocation location)
        {
            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            return syntax is InterfaceDeclarationSyntax;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax,
                                                                  IParsedCSharpDocument document,
                                                                  IFeatureContext context,
                                                                  InteractionLocation location)
        {
            var interfaceSyntax = syntax as InterfaceDeclarationSyntax;

            return CreateSuggestion("Generate an implementation of " + interfaceSyntax.Identifier.ValueText).AsList();
        }

        string GetDefaultFolderPath(string interfaceFolderPath, string projectFolderPath)
        {
            if (interfaceFolderPath.StartsWith(projectFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                var length = projectFolderPath.Length;

                if (projectFolderPath.EndsWith("/", StringComparison.Ordinal)
                    || projectFolderPath.EndsWith("\\", StringComparison.Ordinal))
                {
                    length = projectFolderPath.Length + 1;
                }

                interfaceFolderPath = interfaceFolderPath.Remove(0, length); // Remove the trailing "/"

                if (interfaceFolderPath.StartsWith("/", StringComparison.Ordinal)
                    || interfaceFolderPath.StartsWith("\\", StringComparison.Ordinal))
                {
                    interfaceFolderPath = interfaceFolderPath.Substring(1, interfaceFolderPath.Length - 1);
                }
            }
            else
            {
                interfaceFolderPath = "";
            }

            return interfaceFolderPath;
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax,
                                                       IParsedCSharpDocument document,
                                                       IFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            var interfaceSyntax = syntax as InterfaceDeclarationSyntax;

            context.Project.TryGetCompilation(out var compilation);

            var model = compilation.GetSemanticModel(document.GetSyntaxTree());

            var interfaceSymbol = model.GetDeclaredSymbol(interfaceSyntax);

            var interfaceName = interfaceSyntax.Identifier.ValueText;
            var defaultClassName = interfaceName.StartsWith("i", StringComparison.OrdinalIgnoreCase) ? interfaceName.Substring(1, interfaceName.Length - 1) : "";

            var otherProjects = context.Project.Solution.Projects.Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == context.Project.Id));

            var projects = new List<Project>()
            {
                context.Project
            };
            projects.AddRange(otherProjects);
            
            var interfaceFolderPath = new FileInfo(interfaceSyntax.SyntaxTree.FilePath).DirectoryName;
            var projectFolderPath = new FileInfo(context.Project.FilePath).DirectoryName;

            interfaceFolderPath = GetDefaultFolderPath(interfaceFolderPath, projectFolderPath);
            IReadOnlyList<IWorkUnit> generateCodeFiles(GenerateCodeFilesResult result)
            {
                var interfaceType = model.GetDeclaredSymbol(interfaceSyntax);
                var folderPath = result.FolderPath;
                var interfaceName = result.Name;
                var project = result.SelectedProject;
                return Generate(context.Workspace, interfaceType, folderPath, interfaceName, project);
            }

            var workUnit = new GenerateCodeFilesWorkUnit(defaultClassName,
                                                         context.Project,
                                                         projects,
                                                         interfaceFolderPath,
                                                         "Generate Interface Implementation",
                                                         "Enter the name of the new class to implement " + interfaceSymbol.Name,
                                                         "https://docs.mfractor.com/csharp/code-actions/generate-interface-implementation/",
                                                         ProjectSelectorMode.Single,
                                                         generateCodeFiles);

            return workUnit.AsList();
        }

        IReadOnlyList<IWorkUnit> Generate(CompilationWorkspace workspace, INamedTypeSymbol interfaceType, string folderPath, string interfaceName, Project project)
        {
            var classDeclaration = ClassDeclarationGenerator.GenerateSyntax(interfaceName, interfaceType as INamedTypeSymbol, Enumerable.Empty<MemberDeclaration>());

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

            var outputPath = Path.Combine(folderPath, interfaceName + ".cs");

            return new CreateProjectFileWorkUnit(unit.ToString(), outputPath, project.GetIdentifier()).AsList();
        }
    }
}
