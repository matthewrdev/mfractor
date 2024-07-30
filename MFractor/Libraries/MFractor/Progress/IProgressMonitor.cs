using System;
using System.Threading;

namespace MFractor.Progress
{
    public enum ProgressMonitorMode
    {
        Progress,
        ReportOnly,
    }

    /// <summary>
    /// A progress monitor to notify the caller of a job/tasks progress.
    /// </summary>
    public interface IProgressMonitor : IDisposable
    {
        /// <summary>
        /// Sets the progress using the <paramref name="progressStatus"/>.
        /// </summary>
        /// <param name="progressStatus"></param>
        void SetProgress(ProgressStatus progressStatus);

        /// <summary>
        /// Sets the progress using <paramref name="message"/>, <paramref name="workDone"/> and <paramref name="totalWork"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="workDone"></param>
        /// <param name="totalWork"></param>
        void SetProgress(string message, int workDone, int totalWork);

        /// <summary>
        /// Sets the progress using the <paramref name="message"/> and <paramref name="fraction"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fraction"></param>
        void SetProgress(string message, double fraction);

        /// <summary>
        /// Sets the progress message to <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        void SetMessage(string message);

        /// <summary>
        /// Sets the progress title to <paramref name="title"/>.
        /// </summary>
        /// <param name="title"></param>
        void SetTitle(string title);

        /// <summary>
        /// A <see cref="CancellationToken"/> to notify that the job/task has been cancelled.
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}
