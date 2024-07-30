using System;
using System.Threading.Tasks;
using System.Threading;

namespace MFractor.Concurrency
{
    public class TaskedBackgroundWorkerQueue : IDisposable
    {
        Task previousTask = Task.FromResult(true);
        CancellationTokenSource source = new CancellationTokenSource();
        public CancellationToken CancelToken
        {
            get
            {
                lock (key)
                {
                    return source.Token;
                }
            }
        }

        readonly object key = new object();
        public Task QueueTask(Action action)
        {
            lock (key)
            {
                previousTask = previousTask.ContinueWith(t => action()
                    , source.Token
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                return previousTask;
            }
        }

        public Task<T> QueueTask<T>(Func<T> work)
        {
            lock (key)
            {
                var task = previousTask.ContinueWith(t => work()
                    , source.Token
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                previousTask = task;
                return task;
            }
        }

        public void Cancel()
        {
            lock (key)
            {
                source.Cancel();
            }
        }

        public void Uncancel()
        {
            lock (key)
            {
                source = new CancellationTokenSource();
            }
        }

        public void Dispose()
        {
            source.Cancel();
            if (previousTask != null)
            {
                if (previousTask.IsCompleted)
                {
                    previousTask.Dispose();
                }
                previousTask = null;
            }
        }
    }
}

