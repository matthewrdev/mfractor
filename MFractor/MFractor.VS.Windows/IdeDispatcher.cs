using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDispatcher))]
    class IdeDispatcher : IDispatcher
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public void InvokeOnMainThread(Action action)
        {
            if (action == null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                // Switch to main thread
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            });
        }

        public async System.Threading.Tasks.Task InvokeOnMainThreadAsync(Action action)
        {
            if (action == null)
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                action();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public async Task<TResult> InvokeOnMainThreadAsync<TResult>(Func<TResult> action)
        {
            if (action == null)
            {
                return default;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            TResult result = default;
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return result;
        }
    }
}
