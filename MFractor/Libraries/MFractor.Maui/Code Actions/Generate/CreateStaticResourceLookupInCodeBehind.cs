using System.Collections.Generic;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Generate
{
    class CreateStaticResourceLookupInCodeBehind : GenerateXamlCodeAction
    {
        public enum LookupType
        {
            VisualElement,
            Application,
        }

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_actions.xaml.create_static_resource_lookup_in_code_behind";

        public override string Name => "Create StaticResource Lookup In Code Behind";

        public override string Documentation => "Given a static resource declaration in a XAML file, this code action creates a lookup function in the files code-behind file.";

        [ExportProperty("The default code snippet to use when generating a resource lookup on a VisualElement.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the resource to lookup")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type of the resource to loooup")]
        [CodeSnippetDefaultValue("public $type$ $name$ => ($type$)Resources[\"$name$\"];", "Creates a resource lookup in code behind")]
        public ICodeSnippet LookupSnippet { get; set; }

        [ExportProperty("The default code snippet to use when generating a resource lookup on the apps Application class.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the resource to lookup")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type of the resource to loooup")]
        [CodeSnippetDefaultValue("public static $type$ $name$ => ($type$)Application.Current.Resources[\"$name$\"];", "Creates a static resource lookup")]
        public ICodeSnippet ApplicationLookupSnippet { get; set; }

        public override bool CanExecute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var key = syntax.GetAttributeByName("x:Key");
            if (key == null || !key.HasValue)
            {
                return false;
            }

            return context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol != null;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var suggestions = new List<ICodeActionSuggestion>
            {
                CreateSuggestion("Create a resource lookup in code-behind using an instance property.", LookupType.VisualElement)
            };

            if (SymbolHelper.DerivesFrom(document.CodeBehindSymbol, context.Platform.Application.MetaType))
            {
                suggestions.Add(CreateSuggestion("Create a resource lookup in code-behind using a static property.", LookupType.Application));
            }

            return suggestions;
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var snippet = LookupSnippet;

            if (suggestion.IsAction(LookupType.Application))
            {
                snippet = ApplicationLookupSnippet;
            }

            var key = syntax.GetAttributeByName("x:Key").Value.Value;
            var type = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, key);
            snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Type, type.ToString());

            return new InsertSyntaxNodesWorkUnit()
            {
                HostNode = document.CodeBehindSyntax,
                Workspace = context.Workspace,
                Project = context.Project,
                SyntaxNodes = snippet.AsMembersList().Cast<SyntaxNode>().ToList(),
            }.AsList();
        }
    }
}
