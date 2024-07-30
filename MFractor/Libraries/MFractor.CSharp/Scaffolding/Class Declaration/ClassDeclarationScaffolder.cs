using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Code.Formatting;
using MFractor.Code.Scaffolding;
using MFractor.CSharp.CodeGeneration;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.CSharp.Scaffolding
{
    class ClassDeclarationScaffolder : Scaffolder
    {
        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.Code.Scaffolding.csharp.class";

        public override string Name => "Generate Class Declaration";

        public override string Documentation => "The default scaffolder, creates a new C# class file.";

        public override string Criteria => "Activates when the scaffolding input is within a C# project.";

        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        [ImportingConstructor]
        public ClassDeclarationScaffolder(Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                            Lazy<IProjectService> projectService)
        {
            this.formattingPolicyService = formattingPolicyService;
            this.projectService = projectService;
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return context.Project != null;
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            var className = CSharpNameHelper.ConvertToValidCSharpName(input.NameNoExtension);

            return CreateSuggestion("Create a new class named " + className, int.MinValue).AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(CompilationWorkspace workspace, string className, string folderPath, Project project)
        {
            var classDeclaration = ClassDeclarationGenerator.GenerateSyntax(className, string.Empty, Enumerable.Empty<MemberDeclaration>());

            var namespaceName = NamespaceDeclarationGenerator.GetNamespaceFor(project, folderPath);

            var usingSyntax = UsingDirectiveGenerator.GenerateSyntax("System");

            var namespaceSyntax = NamespaceDeclarationGenerator.GenerateSyntax(namespaceName);
            namespaceSyntax = namespaceSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classDeclaration));

            var unit = SyntaxFactory.CompilationUnit().AddUsings(usingSyntax).AddMembers(namespaceSyntax);

            var options = FormattingPolicyService.GetFormattingPolicy(project);

            unit = (CompilationUnitSyntax)Microsoft.CodeAnalysis.Formatting.Formatter.Format(unit, workspace, options.OptionSet);

            return unit;
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            var className = CSharpNameHelper.ConvertToValidCSharpName(input.NameNoExtension);

            var unit = GenerateSyntax(context.Solution.Workspace, className, input.FolderPath, context.Project);

            var outputPath = Path.Combine(input.FolderPath, className + ".cs");

            return new CreateProjectFileWorkUnit(unit.ToString(), outputPath, context.Project.GetIdentifier()).AsList();
        }
    }
}
