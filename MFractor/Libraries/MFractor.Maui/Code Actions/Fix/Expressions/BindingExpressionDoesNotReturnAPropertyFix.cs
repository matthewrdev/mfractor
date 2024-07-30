using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.DataBinding;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class BindingExpressionDoesNotReturnAPropertyFix : FixCodeAction
	{
        enum ActionId
        {
            Replace,
            Rename,
        }

        public override string Documentation => "Replaces a field or method symbol within a binding expression with a suggested property name.";

        public override Type TargetCodeAnalyser => typeof(BindingExpressionDoesNotReturnAPropertyAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.binding_expression_does_not_return_property";

        public override string Name => "Replace Field Or Method Reference With Property";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, 
                                           XmlAttribute syntax, 
                                           IParsedXamlDocument document, 
                                           IXamlFeatureContext context, 
                                           InteractionLocation location)
        {
            var propBundle = issue.GetAdditionalContent<BindingExpressionPropertyBundle>();
            if (propBundle == null)
            {
                return false;
            }

            if (propBundle.PropertySuggestion == null)
            {
                return false;
            }

            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, 
                                                                     XmlAttribute syntax, 
                                                                     IParsedXamlDocument document, 
                                                                     IXamlFeatureContext context, 
                                                                     InteractionLocation location)
        {
            var propBundle = issue.GetAdditionalContent<BindingExpressionPropertyBundle>();

            return new List<ICodeActionSuggestion>()
            {
                CreateSuggestion($"Replace with {propBundle.PropertySuggestion.Name}", ActionId.Replace),
                CreateSuggestion($"Rename {propBundle.PropertySuggestion.Name} to {propBundle.PropertySuggestion}", ActionId.Rename)
            };
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, 
                                                          XmlAttribute syntax, 
                                                          IParsedXamlDocument document, 
                                                          IXamlFeatureContext context, 
                                                          ICodeActionSuggestion suggestion, 
                                                          InteractionLocation location)
        {
            var propBundle = issue.GetAdditionalContent<BindingExpressionPropertyBundle>();

            var expression = propBundle.Expression;

            var fixContent = propBundle.PropertySuggestion.Name;
            var fixSpan = expression.ReferencedSymbolSpan;

            if (expression.Path != null)
            {
                fixSpan = expression.Path.Span;
                fixContent = "Path=" + propBundle.PropertySuggestion.Name;
            }

            return new ReplaceTextWorkUnit(document.FilePath, fixContent, fixSpan).AsList();
        }
	}
}

