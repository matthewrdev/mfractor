using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Documentation;
using MFractor.Maui.CodeGeneration.CSharp;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions
{
    class GeneratePlatformEffectFromXaml : GenerateXamlCodeAction
    {
        public const string Tip = "Use the Generate Effect code action to create an effect and it's platform specific implementations";

        public override string Documentation => Tip;

        public override string Identifier => "com.mfractor.code_actions.xaml.generate_platform_effect";

        public override string Name => "Generate Effect From XAML Declaration";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IPlatformEffectGenerator EffectGenerator { get; set; }

        public override bool CanExecute(XmlNode syntax,
                                        IParsedXamlDocument document,
                                        IXamlFeatureContext context,
                                        InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (symbol != null)
            {
                return false;
            }

            return syntax.Name.LocalName.EndsWith("effect", System.StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax,
                                                                  IParsedXamlDocument document,
                                                                  IXamlFeatureContext context,
                                                                  InteractionLocation location)
        {
            return CreateSuggestion("Generate a new effect named " + syntax.Name.LocalName, 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax,
                                                       IParsedXamlDocument document,
                                                       IXamlFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            var options = FormattingPolicyService.GetFormattingPolicy(context);

            var defaultNamespace = ProjectService.GetDefaultNamespace(context.Project);

            return EffectGenerator.Generate(syntax.Name.LocalName,
                                                 syntax.Name.Namespace,
                                                 document,
                                                 context.Project,
                                                 context.Workspace,
                                                 options.OptionSet,
                                                 defaultNamespace,
                                                 true);
        }
    }
}
