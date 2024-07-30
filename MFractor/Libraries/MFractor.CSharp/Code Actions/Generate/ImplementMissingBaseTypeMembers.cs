//using System.Collections.Generic;
//using MFractor.Code.CodeActions;
//using MFractor.CSharp.CodeGeneration;
//using MFractor.Documentation;
//
//using MFractor.Code.Documents;
//using MFractor.Work.WorkUnits;
//using Microsoft.CodeAnalysis;
//using System.ComponentModel.Composition;
//using Microsoft.CodeAnalysis.CSharp.Syntax;

//namespace MFractor.CSharp.CodeActions
//{
//    class ImplementMissingBaseTypeMembers : CSharpCodeAction
//    {
//        public override CodeActionCategory Category => CodeActionCategory.Generate;

//        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

//        public override string Identifier => "com.mfractor.code_actions.csharp.implement_interface_member";

//        public override string Name => "Implement Missing Base Type Member Definition";

//        public override string Documentation => DocumentationHelper.RetrieveDocumentation(this);

//        [Import]
//        public IMemberFieldGenerator MemberFieldGenerator
//        {
//            get;
//            set;
//        }

//        [Import("Default")]
//        public IInstancePropertyGenerator InstancePropertyGenerator
//        {
//            get;
//            set;
//        }

//        [Import]
//        public IMethodGenerator MethodGenerator
//        {
//            get;
//            set;
//        }

//        [Import]
//        public IBaseConstructorGenerator BaseConstructorGenerator
//        {
//            get;
//            set;
//        }

//        [Import]
//        public IEventHandlerDeclarationGenerator EventHandlerDeclarationGenerator
//        {
//            get;
//            set;
//        }

//        public override bool CanExecute(SyntaxNode syntax,
//                                        ParsedCSharpDocument document,
//                                        IFeatureContext context,
//                                        InteractionLocation location)
//        {

//            var classDeclarationSyntax = syntax is ClassDeclarationSyntax ? syntax as ClassDeclarationSyntax : syntax.Parent as ClassDeclarationSyntax;
//            if (classDeclarationSyntax == null)
//            {
//                return false;
//            }

//            if (classDeclarationSyntax != syntax && syntax.Span.Contains(location.Position))
//            {
//                return false;
//            }

//            if (!context.Project.TryGetCompilation(out var compilation))
//            {
//                return false;
//            }

//            var model = compilation.GetSemanticModel(syntax.SyntaxTree);

//            if (model == null)
//            {
//                return false;
//            }
             

//            return false;
//        }

//        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax,
//                                                                  ParsedCSharpDocument document,
//                                                                  IFeatureContext context,
//                                                                  InteractionLocation location)
//        {
//            return null;
//        }

//        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax,
//                                                        ParsedCSharpDocument document,
//                                                        IFeatureContext context,
//                                                        ICodeActionSuggestion suggestion,
//                                                        InteractionLocation location)
//        {
//            return null;
//        }
//    }
//}
