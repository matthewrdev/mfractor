using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeActions.Refactor
{
    class ConvertNullEqualityToIsNull : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Refactor;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Documentation => "Converts an `== null` equality check to `is null`.";

        public override string Identifier => "com.mfractor.code_actions.csharp.convert_to_is_null";

        public override string Name => "Convert To Is Null";

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var expressionSyntax = GetBinaryExpressionSyntax(syntax);
            if (expressionSyntax == null)
            {
                return false;
            }

            var rightExpression = expressionSyntax.Right as LiteralExpressionSyntax;
            if (rightExpression is null)
            {
                return false;
            }

            return rightExpression.Token.ValueText == "null";
        }

        BinaryExpressionSyntax GetBinaryExpressionSyntax(SyntaxNode syntax)
        {
            if (syntax is BinaryExpressionSyntax binaryExpressionSyntax)
            {
                return binaryExpressionSyntax;
            }
            else if (syntax is IdentifierNameSyntax identifierName
                     && identifierName.Parent is BinaryExpressionSyntax)
            {
                return syntax.Parent as BinaryExpressionSyntax;
            }
            else if (syntax is LiteralExpressionSyntax literalExpressionSyntax
                     && literalExpressionSyntax.Token.ValueText == "null"
                     && literalExpressionSyntax.Parent is BinaryExpressionSyntax)
            {
                return syntax.Parent as BinaryExpressionSyntax;
            }

            return null;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Change to \"is null\"").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var expressionSyntax = GetBinaryExpressionSyntax(syntax);

            return new ReplaceTextWorkUnit()
            {
                Span = expressionSyntax.Span,
                FilePath = document.FilePath,
                Text = $"{expressionSyntax.Left} is null",
            }.AsList();
        }
    }
}