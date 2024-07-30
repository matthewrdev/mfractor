using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Ide.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeActions.Misc
{
    class LocateClassFileInSolutionExplorer : CSharpCodeAction
    {
        readonly Lazy<IProductInformation> productInformation;
        IProductInformation ProductInformation => productInformation.Value;

        [ImportingConstructor]
        public LocateClassFileInSolutionExplorer(Lazy<IProductInformation> productInformation)
        {
            this.productInformation = productInformation;
        }

        public override CodeActionCategory Category => CodeActionCategory.Misc;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp.locate_class_file_in_solution_explorer";

        public override string Name => "Locate Class File In Solution Explorer";

        public override string Documentation => "For the given class declaration under the cursor, locates the file it belongs to in the solution explorer pad.";

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            if (ProductInformation.Product == Product.VisualStudioWindows)
            {
                return false; // Unsupported in Windows.
            }

            if (!(syntax is TypeDeclarationSyntax))
            {
                return false;
            }

            var projectFile = ProjectService.GetProjectFileWithFilePath(context.Project, syntax.SyntaxTree.FilePath);

            return projectFile != null;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var type = "class";

            if (syntax is EnumDeclarationSyntax)
            {
                type = "enum";
            }
            else if (syntax is InterfaceDeclarationSyntax)
            {
                type = "interface";
            }
            else if (syntax is StructDeclarationSyntax)
            {
                type = "struct";
            }

            return CreateSuggestion($"Locate {type} declaration in solution explorer").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var projectFile = ProjectService.GetProjectFileWithFilePath(context.Project, syntax.SyntaxTree.FilePath);

            return new SelectFileInProjectPadWorkUnit(projectFile).AsList();
        }
    }
}
