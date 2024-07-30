using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.CodeSnippets;
using MFractor.Ide.Navigation;
using MFractor.Maui.CodeGeneration.Commands;
using MFractor.Maui.Mvvm;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions.Generate
{
    class InsertViewModelCommand : CSharpCodeAction
    {
        readonly Lazy<IMvvmResolver> mvvmResolver;
        public IMvvmResolver MvvmResolver => mvvmResolver.Value;

        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.insert_view_model_command";

        public override string Name => "Insert ViewModel Command";

        public override string Documentation => "When inside a View Model, this code action generates a new command implementation.";

        [Import]
        public ICommandImplementationGenerator CommandImplementationGenerator { get; set; }

        [Import]
        public IXamlPlatformRepository XamlPlatforms { get; set; }


        [ImportingConstructor]
        public InsertViewModelCommand(Lazy<IMvvmResolver> mvvmResolver)
        {
            this.mvvmResolver = mvvmResolver;
        }

        protected override bool IsAvailableInDocument(IParsedCSharpDocument document, IFeatureContext context)
        {
            return XamlPlatforms.CanResolvePlatform(context.Project);
        }

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var classDeclarationSyntax = (syntax is ClassDeclarationSyntax ? syntax as ClassDeclarationSyntax : syntax.Parent as ClassDeclarationSyntax);
            if (classDeclarationSyntax == null)
            {
                return false;
            }

            if (classDeclarationSyntax != syntax && syntax.Span.Contains(location.Position))
            {
                return false;
            }

            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            var model = compilation.GetSemanticModel(syntax.SyntaxTree);
            if (model == null)
            {
                return false;
            }

            if (!(model.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol classType))
            {
                return false;
            }

            return MvvmResolver.ResolveContextType(context.Document.FilePath) == RelationalNavigationContextType.Implementation;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Create a new command").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax,
                                                       IParsedCSharpDocument document,
                                                       IFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            var snippet = CommandImplementationGenerator.Snippet;

            var platform = XamlPlatforms.ResolvePlatform(context.Project);

            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Type, platform.Command.MetaType);

            return new InsertCodeSnippetWorkUnit()
            {
                CodeSnippet = snippet,
                FilePath = document.FilePath,
                InsertionOffset = location.Position,
                Title = "Create ViewModel Command",
                ConfirmButton = "Create ViewModel Command",
            }.AsList();
        }
    }
}
