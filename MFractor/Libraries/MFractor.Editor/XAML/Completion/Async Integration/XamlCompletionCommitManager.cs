using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Work;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;

namespace MFractor.Editor.XAML.Completion
{
    class XamlCompletionCommitManager : IAsyncCompletionCommitManager
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly ImmutableArray<char> commitChars = new char[] { ' ', '\'', '"', '.', }.ToImmutableArray();

        readonly IWorkEngine workEngine;
        readonly IAnalyticsService analyticsService;

        public XamlCompletionCommitManager(IWorkEngine workEngine,
                                           IAnalyticsService analyticsService)
        {
            this.workEngine = workEngine;
            this.analyticsService = analyticsService;
        }

        public IEnumerable<char> PotentialCommitCharacters => commitChars;

        public bool ShouldCommitCompletion(IAsyncCompletionSession session, SnapshotPoint location, char typedChar, CancellationToken token)
        {
            if (session.Properties.TryGetProperty(XamlCompletionItemPropertyKeys.IsMFractorSession, out bool result))
            {
                return result;
            }

            return false;
        }

        public CommitResult TryCommit(IAsyncCompletionSession session, ITextBuffer buffer, CompletionItem item, char typedChar, CancellationToken token)
        {
            if (item.Properties.TryGetProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent, out string analyticsEvent))
            {
                analyticsService.Track(analyticsEvent);
            }

            if (item.Properties.TryGetProperty(XamlCompletionItemPropertyKeys.CompletionAction, out CompletionAction completionAction))
            {
                try
                {
                    var suggestion = item.Properties.GetProperty<ICompletionSuggestion>(XamlCompletionItemPropertyKeys.CompletionSuggestion);
                    var workUnits = completionAction(session.TextView, buffer, suggestion);

                    workEngine.ApplyAsync(workUnits).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }

                return CommitResult.Handled;
            }

            if (item.Properties.TryGetProperty(XamlCompletionItemPropertyKeys.CaretOffset, out int caretOffset) && caretOffset >= 0)
            {
                var caretLocation = session.ApplicableToSpan.GetStartPoint(buffer.CurrentSnapshot).Position;
                void itemsCommitted(object sender, CompletionItemEventArgs args)
                {
                    session.ItemCommitted -= itemsCommitted;
                    var offset = caretLocation + caretOffset;
                    session.TextView.SetCaretLocation(offset);
                }

                session.ItemCommitted += itemsCommitted;
            }

            return CommitResult.Handled;
        }
    }
}
