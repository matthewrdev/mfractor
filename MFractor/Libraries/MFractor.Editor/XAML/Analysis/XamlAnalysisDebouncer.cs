using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MFractor.Maui.Analysis;
using MFractor.Text;

namespace MFractor.Editor.XAML.Analysis
{
    public class XamlAnalysisDebouncer
    {
        /// <summary>
        /// A heuristic that defers the execution of the resize request to allow for batching.
        /// </summary>
        const int debounceMilliseconds = 250;

        class ScheduleAnalysisRequest : IDisposable
        {
            public ScheduleAnalysisRequest(IXamlAnalyser xamlAnalyser, ITextProvider textProvider, string filePath, Microsoft.CodeAnalysis.ProjectId id, System.Threading.CancellationToken token)
                : this(new WeakReference<IXamlAnalyser>(xamlAnalyser), textProvider, filePath, id, token)
            {
            }

            public ScheduleAnalysisRequest(WeakReference<IXamlAnalyser> xamlAnalyserReference,
                                           ITextProvider textProvider,
                                           string filePath,
                                           Microsoft.CodeAnalysis.ProjectId id,
                                           System.Threading.CancellationToken token)
            {
                this.TextProvider = textProvider;
                this.FilePath = filePath;
                this.Id = id;
                this.Token = token;

                XamlAnalyserReference = xamlAnalyserReference;
                Key = filePath.GetHashCode();

                dispatchTimer = new Timer(debounceMilliseconds);
                dispatchTimer.Start();
            }

            void DispatchTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                OnScheduledAnalysisRequested?.Invoke(this, EventArgs.Empty);
            }

            public WeakReference<IXamlAnalyser> XamlAnalyserReference { get; }

            public int Key { get; }

            readonly Timer dispatchTimer;

            public ITextProvider TextProvider { get; }
            public string FilePath {get;}
            public Microsoft.CodeAnalysis.ProjectId Id {get;}
            public System.Threading.CancellationToken Token {get;}

            public event EventHandler OnScheduledAnalysisRequested;

            public void Reset()
            {
                try
                {
                    dispatchTimer.Elapsed -= DispatchTimer_Elapsed;

                    dispatchTimer.Stop();
                    dispatchTimer.Start();
                }
                finally
                {
                    dispatchTimer.Elapsed += DispatchTimer_Elapsed;
                }
            }

            public void Dispose()
            {
                dispatchTimer.Elapsed -= DispatchTimer_Elapsed;
                dispatchTimer.Dispose();
            }
        }

        readonly object pendingAnalysisRequestsLock = new object();
        readonly List<ScheduleAnalysisRequest> pendingAnalysisRequests = new List<ScheduleAnalysisRequest>();

        void OnScheduledAnalysisRequested(object sender, EventArgs e)
        {
            if (sender is ScheduleAnalysisRequest request)
            {
                RunXamlAnalysis(request);
            }
        }

        void RunXamlAnalysis(ScheduleAnalysisRequest request)
        {
            if (request.XamlAnalyserReference.TryGetTarget(out var xamlAnalyser)
                && xamlAnalyser != null)
            {
                xamlAnalyser.Analyse(request.TextProvider, request.FilePath, request.Id, request.Token);
                RemoveAnalysisRequest(request.Key);
            }
        }

        void RemoveAnalysisRequest(int key)
        {
            lock (pendingAnalysisRequestsLock)
            {
                var items = pendingAnalysisRequests.Where(request => request.Key == key).ToList();
                foreach (var item in items)
                {
                    item.Dispose();
                    pendingAnalysisRequests.Remove(item);
                }
            }
        }

        internal void RequestAnalysis(IXamlAnalyser xamlAnalyser,
                                      ITextProvider textProvider,
                                      string filePath,
                                      Microsoft.CodeAnalysis.ProjectId id,
                                      System.Threading.CancellationToken token)
        {
            ScheduledAnalysis(xamlAnalyser, textProvider, filePath, id, token);
        }

        void ScheduledAnalysis(IXamlAnalyser xamlAnalyser, ITextProvider textProvider, string filePath, Microsoft.CodeAnalysis.ProjectId id, System.Threading.CancellationToken token)
        {
            if (xamlAnalyser is null
                || textProvider is null
                || string.IsNullOrEmpty(filePath)
                || id is null)
            {
                return;
            }

            var key = filePath.GetHashCode();

            lock (pendingAnalysisRequestsLock)
            {
                var existingRequest = pendingAnalysisRequests.FirstOrDefault(r => r.Key == key);
                if (existingRequest != null)
                {
                    existingRequest.Reset();
                    return;
                }
            }

            var request = new ScheduleAnalysisRequest(xamlAnalyser, textProvider, filePath, id, token);
            lock (pendingAnalysisRequestsLock)
            {
                pendingAnalysisRequests.Add(request);
            }

            request.OnScheduledAnalysisRequested += OnScheduledAnalysisRequested;
            request.Reset();
        }
    }
}
