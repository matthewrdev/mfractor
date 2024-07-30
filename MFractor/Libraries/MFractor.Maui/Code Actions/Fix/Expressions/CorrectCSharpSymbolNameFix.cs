using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class CorrectCSharpSymbolNameFix : FixCodeAction
	{
        public override string Documentation => "When a c# symbol reference expression does not resolve, this fix replaces the unknown class name with ";

        public override Type TargetCodeAnalyser => typeof(DotNetSymbolComponentDoesNotResolveAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.correct_csharp_symbol_name";

        public override string Name => "Correct CSharp Symbol Name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, 
                                           XmlAttribute syntax, 
                                           IParsedXamlDocument document, 
                                           IXamlFeatureContext context, 
                                           InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<DotNetSymbolComponentBundle>();

            if (content == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(content.SuggestedClassName))
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
            var content = issue.GetAdditionalContent<DotNetSymbolComponentBundle>();

            return CreateSuggestion("Replace with " + content.SuggestedClassName).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, 
                                                          XmlAttribute syntax, 
                                                          IParsedXamlDocument document, 
                                                          IXamlFeatureContext context, 
                                                          ICodeActionSuggestion suggestion, 
                                                          InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<DotNetSymbolComponentBundle>();

            return new ReplaceTextWorkUnit(document.FilePath, content.SuggestedClassName, content.Expression.SymbolSpan).AsList();
        }
	}
}

