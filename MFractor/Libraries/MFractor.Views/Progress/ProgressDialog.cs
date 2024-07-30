using System.Threading;
using MFractor.Progress;
using Xwt;

namespace MFractor.Views.Progress
{
    public class ProgressDialog : Dialog, IProgressMonitor
    {
        ProgressMonitorControl progressMonitor;

        public ProgressDialog()
        {
            progressMonitor = new ProgressMonitorControl();

            Content = progressMonitor;
        }

        public CancellationToken CancellationToken => progressMonitor.CancellationToken;

        public void SetMessage(string message)
        {
            progressMonitor.SetMessage(message);
        }

        public void SetProgress(ProgressStatus progressStatus)
        {
            progressMonitor.SetProgress(progressStatus);
        }

        public void SetProgress(string message, int workDone, int totalWork)
        {
            progressMonitor.SetProgress(message, workDone, totalWork);
        }

        public void SetProgress(string message, double fraction)
        {
            progressMonitor.SetProgress(message, fraction);
        }

        public void SetTitle(string title)
        {
            progressMonitor.SetTitle(title);
        }
    }
}
