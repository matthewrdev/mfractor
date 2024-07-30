using System.Collections.Generic;
using MFractor.Code.CodeActions;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Code.Documents;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MFractor.Code;

namespace MFractor.CSharp.CodeActions.MEF
{
    class ExportInterfaceImplementation : CSharpCodeAction
    {
        public override CodeActionCategory Category => CodeActionCategory.Generate;

        public override DocumentExecutionFilter Filter => CSharpCodeActionExecutionFilters.SyntaxNode;

        public override string Identifier => "com.mfractor.code_actions.mef.generate.export_interface_implementation";

        public override string Name => "Export Interface Implementation With MEF";

        public override string Documentation => "For an interface that a class implements, this code action adds an MEF export to the class declaration";

        [ExportProperty("The code snippet that exports the interface implementation via MEF.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The interface type being exported")]
        [CodeSnippetDefaultValue("[System.ComponentModel.Composition.Export(typeof($type$))]", "The code snippet to that exports the interface implementation via MEF.")]
        public ICodeSnippet ExportInterfaceSnippet
        {
            get;
            set;
        }

        [ExportProperty("The code snippet that declares the MEF part creation policy.")]
        [CodeSnippetArgument("$policy$", "The policy kind")]
        [CodeSnippetDefaultValue("[System.ComponentModel.Composition.PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.$policy$)]", "The code snippet that declares the MEF part creation policy.")]
        public ICodeSnippet PartCreationPolicySnippet
        {
            get;
            set;
        }

        protected override bool IsAvailableInDocument(IParsedCSharpDocument document, IFeatureContext context)
        {
            if (!context.Project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            return compilation.HasAssembly("System.ComponentModel.Composition");
        }

        public override bool CanExecute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            var identifier = syntax as IdentifierNameSyntax;
            if (identifier == null)
            {
                return false;
            }

            if (!(identifier.Parent.Parent is BaseListSyntax))
            {
                return false;
            }

            return base.CanExecute(syntax, document, context, location);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, InteractionLocation location)
        {
            return base.Suggest(syntax, document, context, location);
        }

        public override IReadOnlyList<IWorkUnit> Execute(SyntaxNode syntax, IParsedCSharpDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return base.Execute(syntax, document, context, suggestion, location);
        }
    }
}
