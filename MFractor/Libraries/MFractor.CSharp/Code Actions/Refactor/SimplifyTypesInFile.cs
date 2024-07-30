using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.CSharp.Services;
using MFractor.Code.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MFractor.CSharp.WorkUnits;
using MFractor.Work;
using MFractor.Code;
using MFractor.Utilities;

namespace MFractor.CSharp.CodeActions
{
    class SimplifyTypesInFile : CSharpCodeAction
    {
        readonly Lazy<ITypeSyntaxSimplifier> typeSyntaxSimplifier;
        public ITypeSyntaxSimplifier TypeSyntaxSimplifier => typeSyntaxSimplifier.Value;

        public override CodeActionCategory Category => CodeActionCategory.Refactor;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Documentation => "Launches the type simpification wizard to simplify all qualified types in the current C# file.";

        public override string Identifier => "com.mfractor.code_actions.csharp.simplify_all_qualified_types";

        public override string Name => "Simplify All Qualified Types";

        [ImportingConstructor]
        public SimplifyTypesInFile(Lazy<ITypeSyntaxSimplifier> typeSyntaxSimplifier)
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
            return CreateSuggestion("Simplify all qualified types in this file").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new SimplifyTypesInFileWorkUnit()
            {
                ProjectFile = document.ProjectFile,
            }.AsList();
        }
    }
}
