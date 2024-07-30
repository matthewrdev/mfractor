//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using MFractor.Code.CodeActions;
//using MFractor.CSharp.CodeGeneration;
//using MFractor.Code.Documents;
//using MFractor.Work;
//using MFractor.Work.WorkUnits;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;

//namespace MFractor.CSharp.CodeActions.Refactor
//{
//    class ReplaceWithEnumerableEmpty : CSharpCodeAction
//    {
//        public override CodeActionCategory Category => CodeActionCategory.Refactor;

//        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

//        public override string Identifier => "com.mfractor.code_actions.replace_with_enumerable_empty";

//        public override string Name => "Replace With Enumerable.Empty";

//        public override string Documentation => "";

//        public override string AnalyticsEvent => Name;

//        [Import]
//        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

//        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
//        {
//            if (!(syntax is IdentifierNameSyntax identifier))
//            {
//                return false;
//            }

//            var @namespace = GetNamespaceDeclaration(identifier);

//            if (@namespace == null)
//            {
//                return false;
//            }

//            var currentName = @namespace.Name.ToString();
//            var folderPathName = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, document.ProjectFile);

//            return currentName != folderPathName;
//        }

//        NamespaceDeclarationSyntax GetNamespaceDeclaration(IdentifierNameSyntax identifier)
//        {
//            var parent = identifier.Parent;
//            var namespaceDeclaration = parent as NamespaceDeclarationSyntax;

//            while (namespaceDeclaration == null && parent != null)
//            {
//                parent = parent.Parent;

//                namespaceDeclaration = parent as NamespaceDeclarationSyntax;
//                if (parent is IdentifierNameSyntax == false && namespaceDeclaration == null)
//                {
//                    break;
//                }
//            }

//            return namespaceDeclaration;
//        }

//        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
//        {
//            return CreateSuggestion("Align namespace to project folder path");
//        }

//        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
//        {
//            var @namespace = GetNamespaceDeclaration(syntax as IdentifierNameSyntax);
//            var namespaceName = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, document.ProjectFile);

//            return new ReplaceTextWorkUnit()
//            {
//                Text = namespaceName,
//                Span = @namespace.Name.Span,
//                FilePath = document.FilePath,
//            };
//        }
//    }
//}
