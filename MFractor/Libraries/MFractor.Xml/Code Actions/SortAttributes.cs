using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Code.Documents;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml.CodeGeneration;

namespace MFractor.Xml.CodeActions
{
    class SortAttributes : XmlCodeAction
	{
        public override string Documentation => "Sorts the XML attributes under the current cursor";

        public override string Identifier => "com.mfractor.code_actions.xml.sort_attributes";

        public override string Name => "Sort Xaml Attributes";

        public override CodeActionCategory Category => CodeActionCategory.Organise;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
		public ISortedAttributeGenerator SortedAttributeGenerator
        {
            get; set;
        }

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
            if (syntax.HasAttributes == false)
            {
                return false;
            }

            if (syntax.Attributes.Count <= 1)
            {
                return false;
            }

            var sortedAttributes = SortedAttributeGenerator.Generate(syntax).ToList();

            for (var i = 0; i < sortedAttributes.Count; ++i) {

                if (syntax.Attributes[i].Name.FullName != sortedAttributes[i].Name.FullName)
                {
                    return true;
                }
            }

            return false;
		}

		public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
		{
            return CreateSuggestion("Sort attributes", 0).AsList();
		}

		public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
            var sortedAttributes = SortedAttributeGenerator.Generate(syntax);

			var newNode = syntax.Clone();
            newNode.Attributes = sortedAttributes.ToList(); 

            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var customPolicy = new OverloadableXmlFormattingPolicy(policy)
            {
                AttributesInNewLine = true
            };

            return new ReplaceXmlSyntaxWorkUnit()
            {
                Existing = syntax,
                New = newNode,
                FilePath = document.FilePath,
                ReplaceChildren = false,
                GenerateClosingTags = false,
                FormattingPolicy = customPolicy
            }.AsList();
		}
	}
}
