using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Images;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Images
{
    class CorrectImageName : FixCodeAction
    {
        public override string Documentation => "When another image asset is named closely to the invalid image asset, this code issue fix replaces the existing name with the correct name.";

        public override Type TargetCodeAnalyser => typeof(DetectMissingImageResource);

        public override string Identifier => "com.mfractor.code_fixes.xaml.correct_image_name";

        public override string Name => "Correct Image Resource name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var imageBundle = issue.GetAdditionalContent<MissingImageResourceBundle>();

            return syntax.HasValue && !string.IsNullOrEmpty(imageBundle.Suggestion);
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var imageBundle = issue.GetAdditionalContent<MissingImageResourceBundle>();

            return CreateSuggestion("Replace with " + imageBundle.Suggestion).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var imageBundle = issue.GetAdditionalContent<MissingImageResourceBundle>();

            return new ReplaceTextWorkUnit(document.FilePath, imageBundle.Suggestion, syntax.Value.Span).AsList();
        }
    }
}
