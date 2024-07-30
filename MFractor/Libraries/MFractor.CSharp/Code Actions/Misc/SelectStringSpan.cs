using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Ide.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.CSharp.CodeActions.Misc
{
    class SelectStringSpan : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Misc;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp.misc.select_string_span";

        public override string Name => "Select String Span";

        public override string Documentation => "When a string literal is beneath the users cursor, selects the full span of the string.";

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var span = GetStringSpan(syntax);

            return span != null;
        }

        TextSpan? GetStringSpan(SyntaxNode syntax)
        {
            if (syntax is LiteralExpressionSyntax literalExpressionSyntax)
            {
                if (literalExpressionSyntax.Token.IsKind(SyntaxKind.StringLiteralExpression)
                    || literalExpressionSyntax.Token.IsKind(SyntaxKind.StringLiteralToken))
                {
                    var span = syntax.Span;

                    return TextSpan.FromBounds(span.Start + 1, span.End - 1);
                }
            }
            else if (syntax is InterpolatedStringTextSyntax)
            {
                return syntax.Span;
            }

            return null;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Select string").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var span = GetStringSpan(syntax);

            return new NavigateToFileSpanWorkUnit(span.Value, document.FilePath, context.Project, true).AsList();
        }
    }
}
