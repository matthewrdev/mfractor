using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Configuration.Attributes;
using MFractor.CSharp.CodeGeneration;
using MFractor.Code.Scaffolding;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;
using MFractor.Code.Formatting;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;

namespace MFractor.CSharp.Scaffolding
{
    class GenerateClassUsingContextualBaseClass : Scaffolder
    {
        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IContextualBaseClassResolver> contextualBaseClassResolver;
        public IContextualBaseClassResolver ContextualBaseClassResolver => contextualBaseClassResolver.Value;

        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.Code.Scaffolding.csharp.contextual_base_class";

        public override string Name => "Generate Class Using Contextual Base Class";

        public override string Documentation => "Creates a new C# class using the most common base class in the provided project path.";

        public override string Criteria => "Activates when the scaffolding input is within a C# project and the target folder path has one or more C# classes that hint .";

        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        [ExportProperty("The number of classes that share a common base class that triggers the scaffolder for a given folder path")]
        public int MatchHeuristic { get; set; } = 3;

        [ImportingConstructor]
        public GenerateClassUsingContextualBaseClass(Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                                     Lazy<IProjectService> projectService,
                                                     Lazy<IContextualBaseClassResolver> contextualBaseClassResolver)
        {
            this.formattingPolicyService = formattingPolicyService;
            this.projectService = projectService;
            this.contextualBaseClassResolver = contextualBaseClassResolver;
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            if (context.Project == null)
            {
                return false;
            }

            return ContextualBaseClassResolver.GetSuggestedBaseClass(context.Project, input.VirtualFolderPath) != null;
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            var baseType = ContextualBaseClassResolver.GetSuggestedBaseClass(context.Project, input.VirtualFolderPath);

            var className = CSharpNameHelper.ConvertToValidCSharpName(input.NameNoExtension);

            return CreateSuggestion("Create a new class named " + className + " that derives from " + baseType.ToString(), int.MaxValue - 30).AsList();
        }

        public CompilationUnitSyntax GenerateSyntax(CompilationWorkspace workspace, string className, INamedTypeSymbol baseTypeSymbol, string folderPath, Project project)
        {
            var classDeclaration = ClassDeclarationGenerator.GenerateSyntax(className, baseTypeSymbol, Enumerable.Empty<MemberDeclaration>());

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
            var baseType = ContextualBaseClassResolver.GetSuggestedBaseClass(context.Project, input.VirtualFolderPath);

            var className = CSharpNameHelper.ConvertToValidCSharpName(input.NameNoExtension);

            var unit = GenerateSyntax(context.Solution.Workspace, className, baseType, input.FolderPath, context.Project);

            var outputPath = Path.Combine(input.FolderPath, className + ".cs");

            return new CreateProjectFileWorkUnit(unit.ToString(), outputPath, context.Project.GetIdentifier()).AsList();
        }
    }
}
