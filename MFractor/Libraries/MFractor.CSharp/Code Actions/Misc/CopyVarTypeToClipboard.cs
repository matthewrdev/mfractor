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

namespace MFractor.CSharp.CodeActions.Misc
{
    class CopyVarTypeToClipboard : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Misc;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp.misc.copy_var_type_to_clipboard";

        public override string Name => "Copy Var Type To Clipboard";

        public override string Documentation => "For the given `var` variable declaration under the cursor, copies the fully qualified name of the symbol.";

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            if (location.Selection != null)
            {
                return false;
            }

            var typeSymbol = GetTypeSymbol(syntax, context);

            return typeSymbol != null;
        }

        ITypeSymbol GetTypeSymbol(SyntaxNode syntax, IFeatureContext context)
        {
            if (!(syntax is IdentifierNameSyntax identifier))
            {
                return null;
            }

            var variableSyntax = GetVariableDeclarationSyntax(identifier);

            if (variableSyntax == null || !identifier.IsVar)
            {
                return null;
            }

            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            if (semanticModel == null)
            {
                return null;
            }

            var type = semanticModel.GetTypeInfo(identifier).Type;

            return type;
        }

        VariableDeclarationSyntax GetVariableDeclarationSyntax(IdentifierNameSyntax identifier)
        {
            var parent = identifier.Parent;
            var variableDeclaration = parent as VariableDeclarationSyntax;

            while (variableDeclaration == null && parent != null)
            {
                parent = parent.Parent;

                variableDeclaration = parent as VariableDeclarationSyntax;
                if (parent is IdentifierNameSyntax == false && variableDeclaration == null)
                {
                    break;
                }
            }

            return variableDeclaration;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Copy type name to clipboard").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var type = GetTypeSymbol(syntax, context);

            return new CopyValueToClipboardWorkUnit()
            {
                Value = type.ToString(),
                Message = $"Copied '{type.ToString()}' to clipboard",
            }.AsList();
        }
    }
}
