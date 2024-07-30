using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.CodeBehindField;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.CodeBehindField
{
    class CorrectCodeBehindFieldName : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(CodeBehindFieldNameContainsInvalidCharactersAnalysis);

        public override string Documentation => "When an x:Name value contains invalid characters, this code fix ";

        public override string Identifier => "com.mfractor.code_fixes.xaml.correct_code_behind_field_name";

        public override string Name => "Correct Code Behind Field Name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Correct x:Name value").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new ReplaceTextWorkUnit(document.FilePath, CSharpNameHelper.ConvertToValidCSharpName(syntax.Value.Value), syntax.Value.Span).AsList();
        }
    }
}

