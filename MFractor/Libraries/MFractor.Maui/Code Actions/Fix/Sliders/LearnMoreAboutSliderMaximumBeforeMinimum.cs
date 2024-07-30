using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Sliders;
using MFractor.Work;
using MFractor.Xml;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;

namespace MFractor.Maui.CodeActions.Fix.Sliders
{
    class LearnMoreAboutSliderMaximumBeforeMinimum : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(SliderMinimumIsSetBeforeMaximum);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.learn_more_about_slider_max_before_min";

        public override string Name => "Learn More About Slider Maximum Before Minimum";

        public override string Documentation => "When initialising a Slider, the maximum value must be place before the minimum to prevent an exception. This code fix opens the documentation that specifies this restriction.";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Learn more").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new OpenUrlWorkUnit("https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/slider#precautions", false).AsList();
        }
    }
}
