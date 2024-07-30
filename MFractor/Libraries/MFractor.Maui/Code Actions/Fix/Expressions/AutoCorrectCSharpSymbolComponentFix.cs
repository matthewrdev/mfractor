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
    class AutoCorrectCSharpSymbolComponentFix : FixCodeAction
	{
        public override string Documentation => "Fix the name of a C# symbol reference";

        public override Type TargetCodeAnalyser => typeof(Analysis.DotNetSymbolComponentDoesNotResolveAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.autocorrect_csharp_symbol";

        public override string Name => "Autocorrect Symbol In Markup Expression";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public const int AutoCorrectClassNameFix = 1;
		public const int AutoCorrectMemberNameFix = 2;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<DotNetSymbolComponentBundle>();

            return content != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<DotNetSymbolComponentBundle>();

            var suggestions = new List<ICodeActionSuggestion>();

            if (!string.IsNullOrEmpty(content.SuggestedClassName))
            {
                suggestions.Add(CreateSuggestion($"Replace with {content.SuggestedClassName}.", AutoCorrectClassNameFix));
            }

            if (!string.IsNullOrEmpty(content.SuggestedMemberName))
            {
                suggestions.Add(CreateSuggestion($"Replace with {content.SuggestedMemberName}.", AutoCorrectMemberNameFix));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<DotNetSymbolComponentBundle>();

            var span = suggestion.ActionId == AutoCorrectClassNameFix ? content.ClassSpan : content.MemberSpan;
            var name = suggestion.ActionId == AutoCorrectClassNameFix ? content.SuggestedClassName : content.SuggestedMemberName;

            return new ReplaceTextWorkUnit()
            {
                FilePath = document.FilePath,
                Text = name,
                Span = span,
            }.AsList();
        }
	}
}

