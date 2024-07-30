using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Work;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.Completion
{
    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [Name("MFractor Xaml Code Completion")]
    [ContentType(ContentTypes.Xaml)]
    [Order(After = "default")]
    class XamlCompletionCommitManagerProvider : IAsyncCompletionCommitManagerProvider
    {
        [ImportingConstructor]
        public XamlCompletionCommitManagerProvider(IWorkEngine workEngine, IAnalyticsService analyticsService)
        {
            this.workEngine = workEngine;
            this.analyticsService = analyticsService;
        }

        readonly IDictionary<ITextView, IAsyncCompletionCommitManager> cache = new Dictionary<ITextView, IAsyncCompletionCommitManager>();

        readonly IWorkEngine workEngine;

        readonly IAnalyticsService analyticsService;

        public IAsyncCompletionCommitManager GetOrCreate(ITextView textView)
        {
            if (cache.TryGetValue(textView, out var itemSource))
                return itemSource;

            var manager = new XamlCompletionCommitManager(workEngine, analyticsService);

            BindTextViewLifecycleEvents(textView, manager);
            cache.Add(textView, manager);
            return manager;
        }

        void BindTextViewLifecycleEvents(ITextView textView, XamlCompletionCommitManager manager)
        {
            void textViewClosed(object sender, EventArgs e)
            {
                cache.Remove(textView); // clean up memory as files are closed
                textView.Closed -= textViewClosed;
            }

            textView.Closed += textViewClosed;
        }
    }
}
