using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Maui.WorkUnits;
using MFractor.Progress;
using MFractor.Work;

namespace MFractor.Views.MVVMWizard.Settings
{
    class ConfigureProjectMvvmSettingsWorkUnitHandler : WorkUnitHandler<ConfigureProjectMvvmSettingsWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ConfigureProjectMvvmSettingsWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                                           Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        public IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override async Task<IWorkExecutionResult> OnExecute(ConfigureProjectMvvmSettingsWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    var dialog = new ProjectMvvmSettingsDialog();
                    dialog.Load(workUnit.ProjectIdentifier, workUnit.Platform);
                    dialog.MvvmOptionsSaved += (sender, args) =>
                    {
                        workUnit.OnCompleted(args.ProjectIdentifier, args.Settings);
                    };

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
