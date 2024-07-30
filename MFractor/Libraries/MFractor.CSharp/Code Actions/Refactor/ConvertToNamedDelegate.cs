//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.Linq;
//using MFractor.Code.CodeActions;
//using MFractor.CSharp.Services;
//using MFractor.Code.Documents;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using MFractor.CSharp.WorkUnits;
//using MFractor.Work;
//using MFractor.Work.WorkUnits;

//namespace MFractor.CSharp.CodeActions
//{
//    class ConvertToNamedDelegate : CSharpCodeAction
//    {
//        public override CodeActionCategory Category => CodeActionCategory.Refactor;

//        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

//        public override string Documentation => "Converts an Action or Func declaration to a named delegate.";

//        public override string Identifier => "com.mfractor.code_actions.convert_to_named_delegate";

//        public override string Name => "Convert To Named Delegate";

//        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
//        {
//            var assignmentSyntax = GetAssignmentExpressionSyntax(syntax);
//            if (assignmentSyntax == null)
//            {
//                return false;
//            }

//            var isEventBinding = assignmentSyntax.OperatorToken.ValueText == "+=" || assignmentSyntax.OperatorToken.ValueText == "-=";

//            return isEventBinding;
//        }

//        AssignmentExpressionSyntax GetAssignmentExpressionSyntax(SyntaxNode syntax)
//        {
//            var assignmentSyntax = syntax as AssignmentExpressionSyntax;
//            if (assignmentSyntax != null)
//            {
//                return assignmentSyntax;
//            }

//            var identifierNameSyntax = syntax as IdentifierNameSyntax;

//            if (identifierNameSyntax == null)
//            {
//                return null;
//            }

//            return identifierNameSyntax.Parent as AssignmentExpressionSyntax;
//        }

//        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
//        {
//            var assignmentSyntax = GetAssignmentExpressionSyntax(syntax);
//            var content = "+=";

//            if (assignmentSyntax.OperatorToken.ValueText == "+=")
//            {
//                content = "-=";
//            }

//            return CreateSuggestion($"Change to {content}");
//        }

//        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
//        {
//            var assignmentSyntax = GetAssignmentExpressionSyntax(syntax);

//            var content = "+=";

//            if (assignmentSyntax.OperatorToken.ValueText == "+=")
//            {
//                content = "-=";
//            }

//            return new ReplaceTextWorkUnit()
//            {
//                Span = assignmentSyntax.OperatorToken.Span,
//                FilePath = document.FilePath,
//                Text = content,
//            };
//        }
//    }
//}
