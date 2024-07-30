using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.Xml.CodeActions
{
    class ExpandNodeWithClosingTag : XmlCodeAction
	{
        public override string Documentation => "When a Xaml node is self-closing (EG `<MyNode/>`), this organise refactoring allows a developer to generate a closing tag for the node. For example, the node `<MyNode/>` would become `<MyNode> ... </MyNode>` after applying this refactoring.";

        public override string Identifier => "com.mfractor.code_actions.xml.expand_node_with_closing_tag";

        public override string Name => "Expand Node";

        public override CodeActionCategory Category => CodeActionCategory.Organise;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

		protected override bool IsFeatureContextStateValid(IFeatureContext context)
		{
			return true;
		}

		public override bool IsAvailableInDocument(IParsedXmlDocument document, IFeatureContext context)
		{
			return true;
		}

        public override bool CanExecute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
		{
			return syntax.IsSelfClosing;
		}

		public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
		{
            return CreateSuggestion("Expand into opening and closing tag", 0).AsList();
		}

		public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
			var newElement = syntax.Clone();
			newElement.IsSelfClosing = false;

            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var customPolicy = new OverloadableXmlFormattingPolicy(policy)
            {
                AttributesInNewLine = true
            };

            return new ReplaceXmlSyntaxWorkUnit()
            {
                New = newElement,
                Existing = syntax,
                FilePath = document.FilePath,
                ReplaceChildren = false,
                GenerateClosingTags = true,
                FormattingPolicy = customPolicy
            }.AsList();
		}
	}
}
