using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Styles;
using MFractor.Maui.Styles;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Styles
{
    class ElementCanUseAvailableStyleFix : FixCodeAction
    {
        public override string Documentation => "When a XAML element does not have a style applied and an available style can be used in place of it's existing attributes, applies that style and removes the redundant property assignments.";

        public override Type TargetCodeAnalyser => typeof(ElementCanUseAvailableStyle);

        public override string Identifier => "com.mfractor.code_fixes.xaml.element_can_use_available_style";

        public override string Name => "Apply Available Style";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        readonly Lazy<IApplyStyleRefactoring> applyStyleRefactoring;
        public IApplyStyleRefactoring ApplyStyleRefactoring => applyStyleRefactoring.Value;

        [ImportingConstructor]
        public ElementCanUseAvailableStyleFix(Lazy<IApplyStyleRefactoring> applyStyleRefactoring)
        {
            this.applyStyleRefactoring = applyStyleRefactoring;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ElementCanUseAvailableStyleBundle>();

            return bundle != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ElementCanUseAvailableStyleBundle>();

            var suggestions = new List<ICodeActionSuggestion>();

            for (var i = 0; i < bundle.Styles.Count; ++i)
            {
                var style = bundle.Styles[i];
                suggestions.Add(CreateSuggestion($"Apply style '{style.Name}'", i));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<ElementCanUseAvailableStyleBundle>();

            var style = bundle.Styles[suggestion.ActionId];

            return ApplyStyleRefactoring.ApplyStyle(context.Platform, syntax, style, document.FilePath);
        }
    }
}