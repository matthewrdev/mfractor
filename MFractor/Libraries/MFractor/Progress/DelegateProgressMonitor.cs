using System;
using System.Threading;

namespace MFractor.Progress
{
    /// <summary>
    /// A progress monitor accepts a delegate to handle the progress status updates.
    /// </summary>
    public class DelegateProgressMonitor : IProgressMonitor
    {
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        readonly Action<string> progressMessageCallback;
        readonly Action<string> progressTitleCallback;
        readonly Action<string, int, int> progressCallback;

        readonly StubProgressMonitor stub = new StubProgressMonitor();
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public DelegateProgressMonitor(Action<string, int, int> progressCallback,
                                       Action<string> progressMessageCallback,
                                       Action<string> progressTitleCallback)
        {
            this.progressCallback = progressCallback ?? stub.SetProgress;
            this.progressMessageCallback = progressMessageCallback ?? stub.SetMessage;
            this.progressTitleCallback = progressTitleCallback;
        }

        public DelegateProgressMonitor(Action<ProgressStatus> progressCallback)
        {
            if (progressCallback == null)
            {
                throw new ArgumentNullException(nameof(progressCallback));
            }

            this.progressCallback = (string message, int workDone, int totalWork) =>
            {
                progressCallback(new ProgressStatus(message, workDone, totalWork));
            };
        }

        public DelegateProgressMonitor(Action<string, double> progressCallback)
        {
            if (progressCallback == null)
            {
                throw new ArgumentNullException(nameof(progressCallback));
            }

            this.progressCallback = (string message, int workDone, int totalWork) =>
            {
                progressCallback(message, (double)workDone / (double)totalWork);
            };
        }

        /// <summary>
        /// Cancels the current task.
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Sets the progress using the <paramref name="progressStatus"/>.
        /// </summary>
        /// <param name="progressStatus"></param>
        public void SetProgress(ProgressStatus progressStatus)
        {
            progressCallback(progressStatus.Description, progressStatus.WorkDone, progressStatus.TotalWork);
        }

        /// <summary>
        /// Sets the progress using <paramref name="message"/>, <paramref name="workDone"/> and <paramref name="workRemaining"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workDone"></param>
        /// <param name="workRemaining"></param>
        public void SetProgress(string message, int workDone, int workRemaining)
        {
            progressCallback(message, workDone, workRemaining);
        }

        /// <summary>
        /// Sets the progress using the <paramref name="message"/> and <paramref name="fraction"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fraction"></param>
        public void SetProgress(string message, double fraction)
        {
            progressCallback(message, (int)Math.Ceiling(fraction * 100), 100);
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Sets the progress message to <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        public void SetMessage(string message)
        {
            progressMessageCallback(message);
        }

        public void SetTitle(string title)
        {
            progressTitleCallback(title);
        }
    }
}
