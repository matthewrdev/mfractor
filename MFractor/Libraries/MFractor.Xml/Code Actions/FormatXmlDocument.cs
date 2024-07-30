using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Configuration.Attributes;
using MFractor.Code.Documents;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using MFractor.Xml.CodeGeneration;
using MFractor.Code;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;

namespace MFractor.Xml.CodeActions
{
    class FormatXmlDocument : XmlCodeAction
    {
        public override string Documentation => "Applies the xml formatting policy to the entire document. It will correctly indent nodes, sort attributes, align attributes under the parent node.";

        public override string Identifier => "com.mfractor.code_actions.xml.format_document";

        public override string Name => "Format Xaml Document";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override CodeActionCategory Category => CodeActionCategory.Organise;

		[ExportProperty("Should attributes be placed onto separate lines?")]
		public bool AttributesOnSeparateLines { get; set; } = false;

        [Import]
        public ISortedAttributeGenerator SortedAttributeGenerator { get; set; }

        enum AttributeFormatMode
        {
            Default,
            SeparateLines,
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
            return syntax.IsRoot;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, InteractionLocation location)
        {
            return new List<ICodeActionSuggestion>()
            {
                CreateSuggestion("Format Document", AttributeFormatMode.Default),
                CreateSuggestion("Format Document (attributes on separate lines)", AttributeFormatMode.SeparateLines),
            };
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var newRoot = Format(syntax);

            var defaultPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var policy = new OverloadableXmlFormattingPolicy(defaultPolicy);

            if (AttributesOnSeparateLines || suggestion.IsAction(AttributeFormatMode.SeparateLines))
            {
                policy.MaxAttributesPerLine = 1;
            }

            return new ReplaceXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                Existing = document.GetSyntaxTree().Root,
                GenerateClosingTags = true,
                ReplaceChildren = true,
                New = newRoot,
                FormattingPolicy = policy,
            }.AsList();
        }

        XmlNode Format(XmlNode node)
        {
            var copy = node.Clone(false, false);
            
            if (node.HasAttributes)
            {
                copy.Attributes = SortedAttributeGenerator.Generate(node.Attributes).ToList();
            }

            if (node.HasChildren)
            {
                foreach (var c in node.Children)
                {
                    copy.AddChildNode(Format(c));
                }
            }

            return copy;
        }
   }
}
