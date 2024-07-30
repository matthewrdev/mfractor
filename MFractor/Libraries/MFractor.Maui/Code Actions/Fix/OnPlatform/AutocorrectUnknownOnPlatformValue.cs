using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.OnPlatform;
using MFractor.Work.WorkUnits;
using MFractor.Work;
using MFractor.Xml;
using MFractor.Code;
using MFractor.Utilities;

namespace MFractor.Maui.CodeActions.Fix.OnPlatform
{
    class AutocorrectUnknownOnPlatformValue : FixCodeAction
	{
        public override string Documentation => "When the deveoper provides an unknown value to the Platform value for an On element, this code fix replaces it with one of the declared platforms defined by the Device class.";

        public override Type TargetCodeAnalyser => typeof(UnknownOnPlatformValue);

        public override string Identifier => "com.mfractor.code_fixes.xaml.autocorrect_unknown_onplatform_value";

        public override string Name => "Auto-Correct Unknown OnPlatform Value";

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