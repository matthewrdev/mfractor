using System;
using System.Threading.Tasks;

namespace MFractor
{
    /// <summary>
    /// The <see cref="IDispatcher"/> is used to invoke code on the main/UI thread in the current application environment.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Invokes the given <paramref name="action"/> on the main/UI thread.
        /// <para/>
        /// Note: This action does not occur synchronously, the caller code will immediately continue execution and the <paramref name="action"/> will be invoked 
        /// </summary>
        void InvokeOnMainThread(Action action);

        /// <summary>
        /// Invokes the given <paramref name="action"/> on the main/UI thread, allowing the calling code to await the method for "synchronous" execution.
        /// </summary>
        Task InvokeOnMainThreadAsync(Action action);

        /// <summary>
        /// Invokes the given <paramref name="action"/> on the main/UI thread, allowing the calling code to await the method for a <typeparamref name="TResult"/> for "synchronous" execution.
        /// </summary>
        Task<TResult> InvokeOnMainThreadAsync<TResult>(Func<TResult> action);
    }
}
