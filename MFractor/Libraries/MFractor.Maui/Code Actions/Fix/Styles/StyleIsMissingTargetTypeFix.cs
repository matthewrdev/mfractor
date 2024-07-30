using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Styles
{
    class StyleIsMissingTargetTypeFix : FixCodeAction
	{
		public override string Documentation => "When a style is missing a `TargetType` attribute or property setter node, this fix will insert an empty target type attribute.";

        public override Type TargetCodeAnalyser => typeof(Analysis.StyleIsMissingTargetTypeAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.style_is_missing_target_type";

		public override string Name => "Add Missing Target Type Attribute";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
		{
            return syntax.GetAttribute(attr => attr.Name.LocalName == "TargetType") == null 
                         && syntax.GetChildNode(node => node.Name.LocalName == "TargetType") == null;
		}

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
		{
            return CreateSuggestion($"Add missing TargetType attribute", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
		{
            return new InsertXmlSyntaxWorkUnit(new XmlAttribute("TargetType", ""), syntax, document.FilePath).AsList();
		}
	}
}
