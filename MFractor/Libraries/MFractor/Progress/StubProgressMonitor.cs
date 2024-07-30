using System;
using System.Threading;

namespace MFractor.Progress
{
    /// <summary>
    /// A <see cref="IProgressMonitor"/> implementation that does nothing.
    /// </summary>
    public class StubProgressMonitor : IProgressMonitor
    {
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        /// <summary>
        /// Sets the progress using the <paramref name="progressStatus"/>.
        /// </summary>
        /// <param name="progressStatus"></param>
        public void SetProgress(ProgressStatus progressStatus)
        {
            // Do nothing
        }

        /// <summary>
        /// Sets the progress using <paramref name="message"/>, <paramref name="workDone"/> and <paramref name="workRemaining"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workDone"></param>
        /// <param name="workRemaining"></param>
        public void SetProgress(string message, int workDone, int workRemaining)
        {
            // Do nothing
        }

        /// <summary>
        /// Sets the progress using the <paramref name="message"/> and <paramref name="fraction"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fraction"></param>
        public void SetProgress(string message, double fraction)
        {
            // Do nothing
        }

        /// <summary>
        /// Sets the progress message to <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        public void SetMessage(string message)
        {
            // Do nothing.
        }

        public void SetTitle(string title)
        {
            // Do nothing
        }

        /// <summary>
        /// Cancels the current task.
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
        }
    }
}