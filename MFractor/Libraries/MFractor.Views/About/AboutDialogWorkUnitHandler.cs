using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor;

namespace MFractor.Views.About
{
    class AboutDialogWorkUnitHandler : WorkUnitHandler<AboutDialogWorkUnit>
    {
        [ImportingConstructor]
        public AboutDialogWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                         Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override Task<IWorkExecutionResult> OnExecute(AboutDialogWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               var dialog = new AboutDialog
               {
                   InitialLocation = Xwt.WindowLocation.CenterParent
               };
               dialog.Run(RootWindowService.RootWindowFrame);
           });

            return default;
        }
    }
}
