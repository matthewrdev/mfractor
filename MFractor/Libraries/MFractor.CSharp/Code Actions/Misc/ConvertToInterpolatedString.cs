using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.CSharp.CodeActions.Misc
{
    class ConvertToInterpolatedString : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Misc;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp.misc.make_interpolated_string";

        public override string Name => "Convert To Interpolated String";

        public override string Documentation => "Converts the string literal under the cursor to an interpolated string.";

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

            return null;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"To interpolated string").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var span = GetStringSpan(syntax);

            return new InsertTextWorkUnit("$", syntax.Span.Start, document.FilePath).AsList();
        }
    }
}
