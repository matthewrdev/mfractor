using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Maui.CodeActions
{
    class DisableAnalyserCodeAction : XamlCodeAction
    {
        readonly Lazy<IAnalysisResultStore> analysisResultStore;
        public IAnalysisResultStore AnalysisResultStore => analysisResultStore.Value;

        readonly Lazy<ICodeAnalysisOptions> codeAnalysisOptions;
        public ICodeAnalysisOptions CodeAnalysisOptions => codeAnalysisOptions.Value;

        public override CodeActionCategory Category => CodeActionCategory.Misc;

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlDocument;

        public override string Identifier => "com.mfractor.code_actions.xaml.disable_analyser";

        public override string Name => "Disable Xaml Analyser";

        public override string Documentation => "Enables the user to easily disable a given XAML diagnostic.";

        [ImportingConstructor]
        public DisableAnalyserCodeAction(Lazy<IAnalysisResultStore> analysisResultStore,
                                         Lazy<ICodeAnalysisOptions> codeAnalysisOptions)
        {
            this.analysisResultStore = analysisResultStore;
            this.codeAnalysisOptions = codeAnalysisOptions;
        }

        protected override bool IsAvailableInDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return false; // Temporarily disabled.

            //return AnalysisResultStore.Retrieve(document).Any();
        }

        public override bool CanExecute(IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var issues = AnalysisResultStore.Retrieve(document).Where(i => !i.IsSilent);

            return issues.Any(i => i.Span.IntersectsWith(location.Position));
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var issues = AnalysisResultStore.Retrieve(document)
                                            .Where(i => !i.IsSilent)
                                            .Where(i => i.Span.IntersectsWith(location.Position))
                                            .DistinctBy(i => i.AnalyserType)
                                            .ToList();


            var suggestions = new List<ICodeActionSuggestion>();

            foreach (var issue in issues)
            {
                suggestions.Add(CreateSuggestion("Disable " + issue.DiagnosticId, issues.IndexOf(issue)));
            }

            return suggestions;
        }

        public override IReadOnlyList<IWorkUnit> Execute(IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var issues = AnalysisResultStore.Retrieve(document)
                                            .Where(i => !i.IsSilent)
                                            .Where(i => i.Span.IntersectsWith(location.Position))
                                            .DistinctBy(i => i.AnalyserType)
                                            .ToList();

            var issue = issues[suggestion.ActionId];

            CodeAnalysisOptions.ToggleAnalyser(issue.Identifier, false);

            return new StatusBarMessageWorkUnit()
            {
                Message = "The XAML diagnostic " + issue.DiagnosticId + " has been disabled. Please close and reopen this document for this to take affect",
            }.AsList();
        }
    }
}
