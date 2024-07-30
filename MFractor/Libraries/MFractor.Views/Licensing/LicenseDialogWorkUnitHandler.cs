using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.Licensing
{
    class LicenseDialogWorkUnitHandler : WorkUnitHandler<LicenseDialogWorkUnit>
    {
        [ImportingConstructor]
        public LicenseDialogWorkUnitHandler(Lazy<IRootWindowService> rootWindowService, Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override Task<IWorkExecutionResult> OnExecute(LicenseDialogWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               var dialog = new LicenseDialog
               {
                   InitialLocation = Xwt.WindowLocation.CenterScreen
               };
               dialog.Run(RootWindowService.RootWindowFrame);
           });

            return default;
        }
    }
}
