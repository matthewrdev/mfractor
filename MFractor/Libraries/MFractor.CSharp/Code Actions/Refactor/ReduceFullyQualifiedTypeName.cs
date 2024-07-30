using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.CSharp.Services;
using MFractor.Code.Documents;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MFractor.Code;
using MFractor.Utilities;

namespace MFractor.CSharp.CodeActions
{
    class ReduceFullyQualifiedTypeName : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Refactor;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Documentation => "When a type reference is qualified with either a full or partial namespace, this refactoring inserts a using statement for the types namespace and removes the namespace component of the type.";

        public override string Identifier => "com.mfractor.code_actions.csharp.simplify_qualified";

        public override string Name => "Simplify Qualified Type";

        readonly Lazy<ITypeSyntaxSimplifier> typeSyntaxSimplifier;
        public ITypeSyntaxSimplifier TypeSyntaxSimplifier => typeSyntaxSimplifier.Value;

        [ImportingConstructor]
        public ReduceFullyQualifiedTypeName(Lazy<ITypeSyntaxSimplifier> typeSyntaxSimplifier)
        {
            this.typeSyntaxSimplifier = typeSyntaxSimplifier;
        }

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var typeSyntax = syntax as TypeSyntax;
            if (typeSyntax == null)
            {
                return false;
            }

            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            var usings = TypeSyntaxSimplifier.GetReducedTypeUsings(typeSyntax, compilation, out var reduced, out var qualifiedNameSyntax);

            return usings != null && usings.Any();
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            context.Project.TryGetCompilation(out var compilation);

            var usings = TypeSyntaxSimplifier.GetReducedTypeUsings(syntax as TypeSyntax, compilation, out var reduced, out var qualifiedNameSyntax);

            var filteredUsings = TypeSyntaxSimplifier.GetDeduplicatedUsings(syntax.SyntaxTree, usings);

            var message = $"Simplify qualified type and introduce usings";

            if (!usings.Select(u => u.ToString()).Distinct().Any())
            {
                message = $"Simplify qualified type";
            }

            return CreateSuggestion(message).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            context.Project.TryGetCompilation(out var compilation);

            var usings = TypeSyntaxSimplifier.GetReducedTypeUsings(syntax as TypeSyntax, compilation, out var reduced, out var qualifiedNameSyntax);

            var filteredUsings = TypeSyntaxSimplifier.GetDeduplicatedUsings(syntax.SyntaxTree, usings);

            var root = syntax.SyntaxTree.GetRoot() as CompilationUnitSyntax;

            var workUnits = new List<IWorkUnit>()
            {
                new ReplaceTextWorkUnit()
                {
                    Span = qualifiedNameSyntax.Span,
                    Text = reduced.ToString(),
                    FilePath = document.FilePath,
                },
            };

            if (filteredUsings != null && filteredUsings.Any())
            {
                var usingsString = Environment.NewLine + string.Join(Environment.NewLine, filteredUsings.Select(fu => fu.ToString()));

                workUnits.Add(new InsertTextWorkUnit(usingsString, root.Usings.Span.End, document.FilePath));
            }

            return workUnits;
        }
    }
}
