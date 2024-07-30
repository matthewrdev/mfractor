using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.EventHandlers;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions
{
    class CorrectNameToMatchingCallbackHandler : FixCodeAction
    {
        public override string Documentation => "Changes the existing event handler method reference to a matching, closely named method from the code behind class.";

        public override Type TargetCodeAnalyser => typeof(EventHandlerDoesNotExistInCodeBehindAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.correct_name_to_matching_callback";

        public override string Name => "Correct Name To Matching Callback";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool IsAvailableInDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return document.CodeBehindSyntax != null;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!syntax.HasValue)
            {
                return false;
            }

            var bundle = issue.GetAdditionalContent<EventHandlerDoesNotExistInCodeBehindBundle>();

            if (bundle == null)
            {
                return false;
            }

            var suggestion = SuggestionHelper.FindBestSuggestion(syntax.Value.Value, bundle.Callbacks.Select(c => c.Name));

            return !string.IsNullOrEmpty(suggestion);
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var bundle = issue.GetAdditionalContent<EventHandlerDoesNotExistInCodeBehindBundle>();

            var suggestion = SuggestionHelper.FindBestSuggestion(syntax.Value.Value, bundle.Callbacks.Select(c => c.Name));

            return CreateSuggestion("Replace with " + suggestion).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var eventSymbol = issue.GetAdditionalContent<IEventSymbol>();

            var options = FormattingPolicyService.GetFormattingPolicy(context);

            var bundle = issue.GetAdditionalContent<EventHandlerDoesNotExistInCodeBehindBundle>();

            var methodName = SuggestionHelper.FindBestSuggestion(syntax.Value.Value, bundle.Callbacks.Select(c => c.Name));

            return new ReplaceTextWorkUnit()
            {
                Span = syntax.Value.Span,
                Text = methodName,
                FilePath = document.FilePath,
            }.AsList();
        }
    }
}
