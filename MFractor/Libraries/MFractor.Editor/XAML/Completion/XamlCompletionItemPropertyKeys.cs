using System;
using System.Collections.Generic;
using MFractor.Work;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion
{
    public delegate IReadOnlyList<IWorkUnit> CompletionAction(ITextView textView, ITextBuffer buffer, ICompletionSuggestion completionSuggestion);

    public static class XamlCompletionItemPropertyKeys
    {
        /// <summary>
        /// The <see cref="MFractor.Editor.XAML.Completion.CompletionAction"/> to invoke when executing the completion.
        /// <para/>
        /// When a completion action is attached to a completion item, the action 
        /// </summary>
        public const string CompletionAction = nameof(CompletionAction);

        /// <summary>
        /// The <see cref="ICompletionSuggestion"/> attached to the completion item.
        /// </summary>
        public const string CompletionSuggestion = nameof(CompletionSuggestion);

        /// <summary>
        /// After the insertion text is injected into the current text buffer, where should the caret be placed?
        /// </summary>
        public const string CaretOffset = nameof(CaretOffset);

        /// <summary>
        /// The string tooltip to display.
        /// </summary>
        public const string TooltipText = nameof(TooltipText);

        /// <summary>
        /// The tooltip object to display.
        /// </summary>
        public const string TooltipModel = nameof(TooltipModel);

        /// <summary>
        /// The analytics event to register when the completion is triggered.
        /// </summary>
        public const string AnalyticsEvent = nameof(AnalyticsEvent);

        /// <summary>
        /// Attached to a completion session to flag that it's an MFractor completion.
        /// </summary>
        public const string IsMFractorSession = nameof(IsMFractorSession);
    }
}
