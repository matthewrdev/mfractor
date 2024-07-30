using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Maui.WorkUnits;
using MFractor.Progress;
using MFractor.Work;

namespace MFractor.Views.MVVMWizard
{
    class MVVMWizardWorkUnitHandler : WorkUnitHandler<MVVMWizardWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public MVVMWizardWorkUnitHandler(Lazy<IWorkEngine> workEngine,
                                         Lazy<IRootWindowService> rootWindowService,
                                         Lazy<IDispatcher> dispatcher)
        {
            this.workEngine = workEngine;
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        public IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override async Task<IWorkExecutionResult> OnExecute(MVVMWizardWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
           {
               try
               {
                   var dialog = new MVVMWizardDialog(workUnit.Projects, workUnit.Platform, workUnit.TargetProject) ;

                   dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
                   dialog.Run(RootWindowService.RootWindowFrame);
               }
               catch (Exception ex)
               {
                   log?.Exception(ex);
               }
           });

            return default;
        }
    }
}
