using System;
using System.Threading.Tasks;
using MFractor.Progress;

namespace MFractor.Work
{
    /// <summary>
    /// A base class for an implementation of an <see cref="IWorkUnitHandler"/> that strongly ties it to a <typeparamref name="TWorkUnit"/>.
    /// <para/>
    /// When implementing work unit handlers, prefer using <see cref="WorkUnitHandler{TWorkUnit}"/> over <see cref="IWorkUnitHandler"/> directly.
    /// </summary>
    /// <typeparam name="TWorkUnit"></typeparam>
    public abstract class WorkUnitHandler<TWorkUnit> : IWorkUnitHandler where TWorkUnit : class, IWorkUnit
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        /// <summary>
        /// The type of <see cref="MFractor.IWorkUnit"/> that this workUnit handler supports.
        /// </summary>
        /// <value>The type of the supported workUnit.</value>
        public Type SupportedWorkUnitType { get; } = typeof(TWorkUnit);

        /// <summary>
        /// Execute the <paramref name="workUnit"/>.
        /// </summary>
        /// <param name="workUnit"></param>
        /// <param name="progressMonitor"></param>
        /// <returns></returns>
        public async Task<IWorkExecutionResult> Execute(IWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var cast = workUnit as TWorkUnit;
            if (cast == null)
            {
                return default;
            }

            Task<IWorkExecutionResult> task;
            try
            {
                task = OnExecute(cast, progressMonitor ?? new StubProgressMonitor());
                if (task == null)
                {
                    return default;
                }

                return await task;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return default;
        }

        /// <summary>
        /// Execute the given <paramref name="workUnit"/>.
        /// <para/>
        /// It is safe to return null for either the <see cref="Task"/> or <see cref="IWorkExecutionResult"/> from this method.
        /// </summary>
        /// <param name="workUnit"></param>
        /// <param name="progressMonitor"></param>
        /// <returns></returns>
        public abstract Task<IWorkExecutionResult> OnExecute(TWorkUnit workUnit, IProgressMonitor progressMonitor);
    }
}
