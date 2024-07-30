using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Thickness;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Thickness
{
    class SimplifyThicknessValue : FixCodeAction
    {
        public override string Documentation => "When a thickness is initialised with a value string, this code fix can reduce the value into a simplier format. For example, a thickness value of `20,0,20,0` would be simplified to `20,0`.";

        public override Type TargetCodeAnalyser => typeof(ThicknessValueCanBeSimplified);

        public override string Identifier => "com.mfractor.code_fixes.xaml.simplify_thickness_value";
         
        public override string Name => "Simplify Thickness Value";
         
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ThicknessValueCanBeSimplifiedBundle>();

            return !string.IsNullOrEmpty(bundle?.SimplifiedThickness);
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ThicknessValueCanBeSimplifiedBundle>();

            return CreateSuggestion($"Simplify thickness to '{bundle.SimplifiedThickness}'", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ThicknessValueCanBeSimplifiedBundle>();

            return new ReplaceTextWorkUnit(document.FilePath, bundle.SimplifiedThickness, syntax.Value.Span).AsList();
        }
    }
}

