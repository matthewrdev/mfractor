using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.DataBinding;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class AutoCorrectNearlyNamedPropertyBinding : FixCodeAction
	{
        public override string Documentation => "";

        public override Type TargetCodeAnalyser => typeof(BindingExpressionResolveAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.correct_property_binding_name";

        public override string Name => "Correct Property Binding Name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();
            if (content == null)
            {
                return false;
            }

            if (content.SuggestedSymbol != null)
            {
                return true;
            }
            else if (string.IsNullOrEmpty(content.SuggestedPath) == false )
            {
                return true;
            }

            return false;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();

            var value = GetReplacementValue(content);

            return CreateSuggestion($"Replace with '{value}'", 0).AsList();
        }

        string GetReplacementValue(BindingAnalysisBundle content)
        {
            var replacement = string.Empty;
            // Check for a matched symbol.
            if (content.SuggestedSymbol != null)
            {
                replacement = content.SuggestedSymbol.Name;
            }
            else if (string.IsNullOrEmpty(content.SuggestedPath) == false)
            {
                replacement = content.SuggestedPath;
            }

            if (!string.IsNullOrEmpty(content.Prefix))
            {
                replacement = content.Prefix + replacement;
            }

            return replacement;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<BindingAnalysisBundle>();

            var value = GetReplacementValue(content);

            return new ReplaceTextWorkUnit(document.FilePath, value, content.Expression.ReferencedSymbolSpan).AsList();
        }
	}
}

