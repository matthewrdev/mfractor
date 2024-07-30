using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.Sliders;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Sliders
{
    class PlaceSlideMaximumBeforeMinimum : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(SliderMinimumIsSetBeforeMaximum);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.place_slider_max_before_min";

        public override string Name => "Place Slider Maximum Before Minimum";

        public override string Documentation => "When initialising a Slider, the maximum value must be place before the minimum to prevent an exception. This code fix rearranges the XAML and places the maximum initialiser before the minimum";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Initialise Maximum before Minimum").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var node = syntax.Parent;

            var minimum = node.GetAttributeByName("Minimum");
            var maximum = node.GetAttributeByName("Maximum");

            var workUnits = new List<IWorkUnit>();

            workUnits.Add(new DeleteXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                Syntaxes = new List<XmlSyntax>()
                {
                    maximum
                }
            });

            workUnits.Add(new InsertTextWorkUnit($"Maximum=\"{maximum.Value}\" ", minimum.Span.Start, document.FilePath));

            return workUnits;
        }
    }
}