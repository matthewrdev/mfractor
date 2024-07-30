using System.Collections.Generic;
using MFractor.Code.Documents;
using MFractor.Code.CodeActions;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Code;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;

namespace MFractor.Xml.CodeActions
{
    class ExpandAttributesOntoNewLines : XmlCodeAction
	{
        public override string Documentation => "Expands the attributes for the given XML element onto new lines";

        public override string Identifier => "com.mfractor.code_actions.xml.expand_attributes_onto_new_lines";

        public override string Name => "Expand Attributes Onto Separate Lines";

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
            if (!syntax.HasAttributes
                 || syntax.Attributes.Count <= 1)
            {
                return false;
            }

            return true;
		}

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
		{
            return CreateSuggestion("Split attributes onto separate lines").AsList();
		}

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var customPolicy = new OverloadableXmlFormattingPolicy(policy);

			customPolicy.MaxAttributesPerLine = 1;

            return new ReplaceXmlSyntaxWorkUnit()
            {
                New = syntax,
                Existing = syntax,
                FilePath = document.FilePath,
                ReplaceChildren = false,
                GenerateClosingTags = !syntax.HasChildren,
                FormattingPolicy = customPolicy
            }.AsList();
		}
	}
}
