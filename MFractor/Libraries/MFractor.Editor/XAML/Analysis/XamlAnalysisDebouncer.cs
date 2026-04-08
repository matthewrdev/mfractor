using System;
using System.Collections.Generic;
using System.Timers;
using MFractor.Maui.Analysis;

namespace MFractor.Editor.XAML.Analysis
{
    public class XamlAnalysisDebouncer
    {
        const int debounceMilliseconds = 250;

        class ScheduleAnalysisRequest : IDisposable
        {
            public ScheduleAnalysisRequest(IXamlAnalyser xamlAnalyser,
                                           string filePath,
                                           Microsoft.CodeAnalysis.ProjectId id,
                                           System.Threading.CancellationToken token)
                : this(new WeakReference<IXamlAnalyser>(xamlAnalyser), filePath, id, token)
            {
            }

            public ScheduleAnalysisRequest(WeakReference<IXamlAnalyser> xamlAnalyserReference,
                                           string filePath,
                                           Microsoft.CodeAnalysis.ProjectId id,
                                           System.Threading.CancellationToken token)
            {
                FilePath = filePath;
                Id = id;
                Token = token;

                XamlAnalyserReference = xamlAnalyserReference;
                Key = filePath;

                dispatchTimer = new Timer(debounceMilliseconds);
                dispatchTimer.Start();
            }

            void DispatchTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                OnScheduledAnalysisRequested?.Invoke(this, EventArgs.Empty);
            }

            public WeakReference<IXamlAnalyser> XamlAnalyserReference { get; }

            public string Key { get; }

            readonly Timer dispatchTimer;

            public string FilePath { get; }
            public Microsoft.CodeAnalysis.ProjectId Id { get; private set; }
            public System.Threading.CancellationToken Token { get; private set; }

            public event EventHandler OnScheduledAnalysisRequested;

            public void Update(Microsoft.CodeAnalysis.ProjectId id,
                               System.Threading.CancellationToken token)
            {
                Id = id;
                Token = token;
            }

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
        readonly Dictionary<string, ScheduleAnalysisRequest> pendingAnalysisRequests = new Dictionary<string, ScheduleAnalysisRequest>(StringComparer.Ordinal);

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
                xamlAnalyser.Analyse(request.FilePath, request.Id, request.Token);
                RemoveAnalysisRequest(request.Key);
            }
        }

        void RemoveAnalysisRequest(string key)
        {
            lock (pendingAnalysisRequestsLock)
            {
                if (pendingAnalysisRequests.TryGetValue(key, out var request))
                {
                    request.OnScheduledAnalysisRequested -= OnScheduledAnalysisRequested;
                    request.Dispose();
                    pendingAnalysisRequests.Remove(key);
                }
            }
        }

        internal void RequestAnalysis(IXamlAnalyser xamlAnalyser,
                                      string filePath,
                                      Microsoft.CodeAnalysis.ProjectId id,
                                      System.Threading.CancellationToken token)
        {
            ScheduledAnalysis(xamlAnalyser, filePath, id, token);
        }

        void ScheduledAnalysis(IXamlAnalyser xamlAnalyser,
                               string filePath,
                               Microsoft.CodeAnalysis.ProjectId id,
                               System.Threading.CancellationToken token)
        {
            if (xamlAnalyser is null
                || string.IsNullOrEmpty(filePath)
                || id is null)
            {
                return;
            }

            lock (pendingAnalysisRequestsLock)
            {
                if (pendingAnalysisRequests.TryGetValue(filePath, out var existingRequest))
                {
                    existingRequest.Update(id, token);
                    existingRequest.Reset();
                    return;
                }
            }

            var request = new ScheduleAnalysisRequest(xamlAnalyser, filePath, id, token);
            lock (pendingAnalysisRequestsLock)
            {
                pendingAnalysisRequests[filePath] = request;
            }

            request.OnScheduledAnalysisRequested += OnScheduledAnalysisRequested;
            request.Reset();
        }
    }
}
