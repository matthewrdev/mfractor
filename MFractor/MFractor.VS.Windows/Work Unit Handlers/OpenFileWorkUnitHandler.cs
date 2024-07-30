using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.VisualStudio.Shell;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class OpenFileWorkUnitHandler : WorkUnitHandler<OpenFileWorkUnit>
    {
        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        private DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        [ImportingConstructor]
        public OpenFileWorkUnitHandler(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(OpenFileWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                DTE.ItemOperations.OpenFile(workUnit.FilePath);
            }).ConfigureAwait(false);

            return default;
        }
    }
}
