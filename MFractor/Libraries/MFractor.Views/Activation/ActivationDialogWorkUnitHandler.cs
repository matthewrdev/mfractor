using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.Activation
{
    class ActivationDialogWorkUnitHandler : WorkUnitHandler<ActivationDialogWorkUnit>
    {
        readonly Lazy<IRootWindowService> rootWindowService;
        public IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public ActivationDialogWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                               Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(ActivationDialogWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
           Dispatcher.InvokeOnMainThread(() =>
           {
               var dialog = new ActivationDialog();
               dialog.OnSuccessfulActivation += (sender, e) =>
               {
                   workUnit.OnSuccessfulActivation?.Invoke();
               };

               dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
               dialog.Run(RootWindowService.RootWindowFrame);
           });


            return default;
        }
    }
}
