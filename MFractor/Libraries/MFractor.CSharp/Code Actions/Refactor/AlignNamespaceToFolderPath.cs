using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeActions;
using MFractor.CSharp.CodeGeneration;
using MFractor.Code.Documents;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MFractor.Code;
using MFractor.Utilities;

namespace MFractor.CSharp.CodeActions.Refactor
{
    public class AlignNamespaceToFolderPath : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Refactor;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.csharp_align_namespace_to_folder_path";

        public override string Name => "Align Namespace To Folder Path";

        public override string Documentation => "For the namespace under the users cursor, this will make the namespace declaration align to the projects default namespace plus the virtual folder path this file is under.";

        public override string AnalyticsEvent => Name;

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            if (!(syntax is IdentifierNameSyntax identifier))
            {
                return false;
            }

            var @namespace = GetNamespaceDeclaration(identifier);

            if (@namespace == null)
            {
                return false;
            }

            var currentName = @namespace.Name.ToString();
            var folderPathName = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, document.ProjectFile);

            return currentName != folderPathName;
        }

        NamespaceDeclarationSyntax GetNamespaceDeclaration(IdentifierNameSyntax identifier)
        {
            var parent = identifier.Parent;
            var namespaceDeclaration = parent as NamespaceDeclarationSyntax;

            while (namespaceDeclaration == null && parent != null)
            {
                parent = parent.Parent;

                namespaceDeclaration = parent as NamespaceDeclarationSyntax;
                if (parent is IdentifierNameSyntax == false && namespaceDeclaration == null)
                {
                    break;
                }
            }

            return namespaceDeclaration;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Align namespace to project folder path").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var @namespace = GetNamespaceDeclaration(syntax as IdentifierNameSyntax);
            var namespaceName = NamespaceDeclarationGenerator.GetNamespaceFor(context.Project, document.ProjectFile);

            return new ReplaceTextWorkUnit()
            {
                Text = namespaceName,
                Span = @namespace.Name.Span,
                FilePath = document.FilePath,
            }.AsList();
        }
    }
}
