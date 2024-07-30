using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace MFractor.VS.Mac
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDispatcher))]
    class IdeDispatcher : IDispatcher
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public void InvokeOnMainThread(Action action)
        {
            try
            {
                if (action != null)
                {
                    AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(action);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public Task InvokeOnMainThreadAsync(Action action)
        {
            if (action == null)
            {
                return Task.CompletedTask;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();

            AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();

                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            });

            return taskCompletionSource.Task;
        }

        public Task<TResult> InvokeOnMainThreadAsync<TResult>(Func<TResult> action)
        {
            if (action == null)
            {
                return Task.FromResult<TResult>(default);
            }

            var taskCompletionSource = new TaskCompletionSource<TResult>();

            AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var result = action();

                    taskCompletionSource.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            });

            return taskCompletionSource.Task;
        }
    }
}
