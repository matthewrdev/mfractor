using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.CSharp.WorkUnits;
using MFractor.Licensing;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.ClassFromClipboard
{
    class CreateClassFromContentWorkUnitHandler : WorkUnitHandler<CreateClassFromContentWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public CreateClassFromContentWorkUnitHandler(Lazy<IWorkEngine> workEngine,
                                               Lazy<IRootWindowService> rootWindowService,
                                               Lazy<ILicensingService> licensingService,
                                               Lazy<IDispatcher> dispatcher)
        {
            this.workEngine = workEngine;
            this.rootWindowService = rootWindowService;
            this.licensingService = licensingService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override async Task<IWorkExecutionResult> OnExecute(CreateClassFromContentWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
          {
              try
              {
                  var dialog = new CreateClassFromContentDialog();

                  dialog.OnApplyResult += async (sender, e) =>
                  {
                      try
                      {
                          await WorkEngine.ApplyAsync(e.WorkUnits);
                      }
                      catch (Exception ex)
                      {
                          log?.Exception(ex);
                      }
                  };

                  dialog.SetWorkUnit(workUnit);
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
