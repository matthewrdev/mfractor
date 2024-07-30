using System;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;

namespace MFractor.Editor.Tooltips
{
    public class CodeIssueFixSelectedEventArgs : EventArgs
    {
        public CodeIssueFixSelectedEventArgs(ICodeActionSuggestion codeActionSuggestion)
        {
            CodeActionSuggestion = codeActionSuggestion ?? throw new ArgumentNullException(nameof(codeActionSuggestion));
        }

        public ICodeActionSuggestion CodeActionSuggestion { get; }
    }

    public class CodeIssueTooltipModel
    {
        public CodeIssueTooltipModel(ICodeIssue codeIssue,
                                     IFeatureContext featureContext,
                                     InteractionLocation interactionLocation)
        {
            CodeIssue = codeIssue;
            FeatureContext = featureContext;
            InteractionLocation = interactionLocation;
        }

        public ICodeIssue CodeIssue { get; }

        public IFeatureContext FeatureContext { get; }

        public InteractionLocation InteractionLocation { get; }

        public string HelpUrl { get; set; }
    }
}
