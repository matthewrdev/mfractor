using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Licensing;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.GenerateCodeFiles
{
    class GenerateCodeFileWorkUnitHandler : WorkUnitHandler<GenerateCodeFilesWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public GenerateCodeFileWorkUnitHandler(Lazy<IWorkEngine> workEngine,
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

        public override async Task<IWorkExecutionResult> OnExecute(GenerateCodeFilesWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
          {
              try
              {
                  var dialog = new GenerateCodeFilesDialog(workUnit.ProjectSelectorMode);

                  dialog.GenerateInterfaceResultEvent += async (sender, e) =>
                  {
                      try
                      {
                          var workUnits = workUnit.GenerateCodeFilesDelegate(e.Result);

                          await WorkEngine.ApplyAsync(workUnits);
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
