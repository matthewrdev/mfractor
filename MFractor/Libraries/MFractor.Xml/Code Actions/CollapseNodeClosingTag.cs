using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.Xml.CodeActions
{
    class CollapseNodeClosingTag : XmlCodeAction
	{
        public override string Documentation => "When a Xaml node has a closing tag and no children (eg: `<MyNode> </MyNode>`), this refactoring allows the developer to remove the closing tag to make the node self-closing. For example, when applied to the node `<MyNode> ... </MyNode>`, the closing tag `</MyNode>` is removed and the element becomes self closing like so: `<MyNode/>`.";

        public override string Identifier => "com.mfractor.code_actions.xml.collapse_node_closing_tag";

		public override string Name => "Collapse Node";

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
            return !syntax.IsSelfClosing && !syntax.HasChildren && !syntax.HasValue;
		}

		public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
		{
            return CreateSuggestion("Collapse closing tag", 0).AsList();
		}

		public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
			var newElement = syntax.Clone();
			newElement.IsSelfClosing = true;

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
                ReplaceChildren = true,
                GenerateClosingTags = false,
                FormattingPolicy = customPolicy,
            }.AsList();
		}
	}
}
