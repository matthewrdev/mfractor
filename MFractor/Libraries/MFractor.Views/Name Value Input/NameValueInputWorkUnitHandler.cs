using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.NameValueInput
{ 
    class NameValueInputWorkUnitHandler : WorkUnitHandler<NameValueInputWorkUnit>
    {
        [ImportingConstructor]
        public NameValueInputWorkUnitHandler(Lazy<IRootWindowService> rootWindowService, Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override Task<IWorkExecutionResult> OnExecute(NameValueInputWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                var dialog = new NameValueInputDialog();
                dialog.SetWorkUnit(workUnit);

                dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
                dialog.Run(RootWindowService.RootWindowFrame);
            });

            return default;
        }
    }
}
