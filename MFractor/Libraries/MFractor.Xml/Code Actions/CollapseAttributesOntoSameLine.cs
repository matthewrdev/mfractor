using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Xml.CodeActions
{
    class CollapseAttributesOntoSameLine : XmlCodeAction
    {
        public override string Documentation => "Collapses the XML attributes for a given XML node onto the same line";

        public override string Identifier => "com.mfractor.code_actions.xml.collapse_attributes_onto_same_line";

        public override string Name => "Collapse Attributes Onto Same Line";

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
            return syntax.HasAttributes && syntax.Attributes.Count > 1;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Collapse attributes onto same line", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var defaultPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var customPolicy = new OverloadableXmlFormattingPolicy(defaultPolicy)
            {
                MaxAttributesPerLine = int.MaxValue
            };

            return new ReplaceXmlSyntaxWorkUnit()
            {
                New = syntax,
                Existing = syntax,
                FilePath = document.FilePath,
                ReplaceChildren = false,
                GenerateClosingTags = true,
                FormattingPolicy = customPolicy
            }.AsList();
        }
    }
}
