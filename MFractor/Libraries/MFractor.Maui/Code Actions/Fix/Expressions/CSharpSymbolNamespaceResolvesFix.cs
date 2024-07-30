using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class CSharpSymbolNamespaceResolvesFix : FixCodeAction
	{
        public override string Documentation => "Generate an xmlns import statement for a missing C# type reference";

        public override Type TargetCodeAnalyser => typeof(DotNetSymbolNamespaceResolvesAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.resolve_expression_csharp_symbol";

        public override string Name => "Import Namespace And Assembly For Unresolved Expression Symbol";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        public const int GenerateImportFix = 1;
		public const int AutoCorrectNameFix = 2;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<DotNetSymbolNamespaceBundle>();

            return content != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var content = issue.GetAdditionalContent<DotNetSymbolNamespaceBundle>();

            var suggestions = new List<ICodeActionSuggestion>();

            // Check for a matched symbol.
            if (content.MatchedSymbol != null)
            {
                suggestions.Add(CreateSuggestion($"Import '{content.MatchedSymbol.ContainingNamespace.ToString()}'", GenerateImportFix));
            }

            // Check for a suggestion.
            if (!string.IsNullOrEmpty(content.Suggestion) && content.Suggestion != syntax.Name?.FullName)
            {
                suggestions.Add(CreateSuggestion($"Replace with '{content.Suggestion}'", AutoCorrectNameFix));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();
            var content = issue.GetAdditionalContent<DotNetSymbolNamespaceBundle>();

            var workUnits = new List<IWorkUnit>();

            switch (suggestion.ActionId)
            {
                case GenerateImportFix:
                    workUnits.AddRange(XamlNamespaceImportGenerator.CreateXmlnsImportStatementWorkUnit(document, context.Platform, content.Expression.Namespace, content.MatchedSymbol, context.Project, xmlPolicy));
                    break;
                case AutoCorrectNameFix:
                    workUnits.Add(new ReplaceTextWorkUnit(document.FilePath, content.Suggestion, content.Expression.NamespaceSpan));
                    break;
            }

            return workUnits;
        }
	}
}

