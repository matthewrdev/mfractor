using System;
using System.Threading;
using MFractor.Progress;
using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac.Progress
{
    /// <summary>
    /// A <see cref="IProgressMonitor"/> implementation that routes to a backing Visual Studio Mac style progress monitor.
    /// </summary>
    public class IdeProgressMonitor : IProgressMonitor
    {
        public ProgressMonitor ProgressMonitor { get; }

        public IdeProgressMonitor(string description = "", ProgressMonitor progressMonitor = null)
        {
            ProgressMonitor = progressMonitor ?? IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor(description, null);
        }

        /// <summary>
        /// A <see cref="P:MFractor.Progress.IProgressMonitor.CancellationToken"/> to notify that the job/task has been cancelled.
        /// </summary>
        public CancellationToken CancellationToken => throw new NotImplementedException();

        public void Dispose()
        {
            ProgressMonitor.Dispose();
        }

        /// <summary>
        /// Sets the progress using the <paramref name="progressStatus"/>.
        /// </summary>
        /// <param name="progressStatus"></param>
        public void SetProgress(ProgressStatus progressStatus)
        {
            SetProgress(progressStatus.Description, progressStatus.WorkDone, progressStatus.TotalWork);
        }

        /// <summary>
        /// Sets the progress using <paramref name="message"/>, <paramref name="workDone"/> and <paramref name="workRemaining"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workDone"></param>
        /// <param name="workRemaining"></param>
        public void SetProgress(string message, int workDone, int workRemaining)
        {
            ProgressMonitor.EndTask();
            ProgressMonitor.BeginTask(message, workDone + workRemaining);
        }

        /// <summary>
        /// Sets the progress using the <paramref name="message"/> and <paramref name="fraction"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fraction"></param>
        public void SetProgress(string message, double fraction)
        {
            SetProgress(message, (int)Math.Ceiling(fraction * 100), 100);
        }

        /// <summary>
        /// Sets the progress message to <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        public void SetMessage(string message)
        {
            ProgressMonitor.Step(message, 0);
        }

        public void SetTitle(string title)
        {
        }
    }
}
