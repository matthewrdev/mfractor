using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Fonts;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Fonts
{
    class AutocorrectEmbeddedFontReference : FixCodeAction
	{
        public override string Documentation => "When a developer accidentally mispells an embedded font reference, this fix will auto-correct it to an exported font.";

        public override Type TargetCodeAnalyser => typeof(UnknownEmbeddedFontReference);

        public override string Identifier => "com.mfractor.code_fixes.xaml.autocorrect_embedded_font_reference";

        public override string Name => "Auto-Correct Embedded Font Reference";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var suggestion = issue.GetAdditionalContent<string>();

            return string.IsNullOrEmpty(suggestion) == false;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var suggestion = issue.GetAdditionalContent<string>();

            return CreateSuggestion("Replace with '" + suggestion + "'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var correction = issue.GetAdditionalContent<string>();

            return new ReplaceTextWorkUnit(document.FilePath, correction, syntax.Value.Span).AsList();
        }
	}
}

