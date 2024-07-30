using System.Linq;
using System.Threading;
using MFractor.Progress;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MFractor.VS.Windows.Services
{
    class StatusBarProgressMonitor : IProgressMonitor
    {
        public CancellationToken CancellationToken => CancellationToken.None;

        private IVsStatusbar StatusBar { get; } = ServiceProvider.GlobalProvider.GetService(typeof(IVsStatusbar)) as IVsStatusbar;

        public StatusBarProgressMonitor()
        {
            Xwt.Application.Invoke(() =>
            {
                object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Build;
                StatusBar.Animation(1, ref icon);
            });
        }

        public void Dispose()
        {
            Xwt.Application.Invoke(() =>
            {
                object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Build;
                StatusBar.Animation(0, ref icon);
            });
        }

        public void SetMessage(string message)
        {
            Xwt.Application.Invoke(() =>
            {
                StatusBar.SetText(message);
            });
        }

        public void SetProgress(ProgressStatus progressStatus)
        {
            SetMessage(progressStatus.Description);
        }

        public void SetProgress(string message, int workDone, int totalWork)
        {
            SetMessage(message);
        }

        public void SetProgress(string message, double fraction)
        {
            SetMessage(message);
        }

        public void SetTitle(string title)
        {
            SetMessage(title);
        }
    }
}
