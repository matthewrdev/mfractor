using System.ComponentModel.Composition;
using System.Threading;
using MFractor.IOC;
using MFractor.Progress;
using Xwt;

namespace MFractor.Views.Progress
{
    public class ProgressMonitorControl : VBox, IProgressMonitor
    {
        [Import]
        public IDispatcher Dispatcher { get; set; }

        Label messageLabel;
        ProgressBar progressBar;

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public ProgressMonitorControl()
        {
            Resolver.ComposeParts(this);

            Build();
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();

        }

        public void Reset()
        {
            cancellationTokenSource = new CancellationTokenSource();
            messageLabel.Text = string.Empty;
            progressBar.Fraction = 0;

        }

        void Build()
        {
            messageLabel = new Label();
            progressBar = new ProgressBar();

            PackStart(messageLabel);
            PackStart(progressBar);
        }

        public void SetProgress(string message, double fraction)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                messageLabel.Text = message;
                progressBar.Fraction = fraction;
            });
        }

        public void SetProgress(string message, int workDone, int totalWork)
        {
            SetProgress(message, (double)workDone / (double)totalWork);
        }

        public void SetProgress(ProgressStatus progressStatus)
        {
            SetProgress(progressStatus.Description, progressStatus.WorkDone, progressStatus.TotalWork);
        }

        public void SetMessage(string message)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                messageLabel.Text = message;
            });
        }

        public void SetTitle(string title)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                SetTitle(title);
            });
        }
    }
}
