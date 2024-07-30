using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.Onboarding
{
    class OnboardingDialogWorkUnitHandler : WorkUnitHandler<OnboardingDialogWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IRootWindowService> rootWindowService;

        public IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public OnboardingDialogWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                               Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(OnboardingDialogWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
          {
              try
              {
                  var dialog = new OnboardingDialog();

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
