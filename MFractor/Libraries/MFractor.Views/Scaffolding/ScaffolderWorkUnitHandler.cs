using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Code.WorkUnits;
using MFractor.Configuration;

using MFractor.Licensing;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.Scaffolding
{
    class ScaffolderWorkUnitHandler : WorkUnitHandler<ScaffolderWorkUnit>, IAnalyticsFeature
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IWorkEngine> workEngine;
        IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IUserOptions> userOptions;
        public IUserOptions UserOptions => userOptions.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public string AnalyticsEvent => "Scaffolder";

        [ImportingConstructor]
        public ScaffolderWorkUnitHandler(Lazy<IWorkEngine> workEngine,
                                         Lazy<IRootWindowService> rootWindowService,
                                         Lazy<ILicensingService> licensingService,
                                         Lazy<IAnalyticsService> analyticsService,
                                         Lazy<IDialogsService> dialogsService,
                                         Lazy<IUserOptions> userOptions,
                                         Lazy<IDispatcher> dispatcher)
        {
            this.workEngine = workEngine;
            this.rootWindowService = rootWindowService;
            this.licensingService = licensingService;
            this.analyticsService = analyticsService;
            this.dialogsService = dialogsService;
            this.userOptions = userOptions;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(ScaffolderWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                try
                {
                    AnalyticsService.Track(this);

                    var dialog = new ScaffolderDialog();
                    dialog.SetScaffoldingContext(workUnit.Context, workUnit.InputValue);
                    async void onConfirmed(object sender, ScaffoldingResultEventArgs e)
                    {
                        if (!LicensingService.IsPaid)
                        {
                            await WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit("The Scaffolder is a Professional-only MFractor feature. Please upgrade or request a trial.", "Scaffolder"));
                            return;
                        }

                        AnalyticsService.Track(e.Scaffolder);

                        var workUnits = e.Scaffolder.ProvideScaffolds(e.ScaffoldingContext, e.ScaffoldingInput, e.ScaffolderState, e.ScaffoldingSuggestion);

                        await WorkEngine.ApplyAsync(workUnits, progressMonitor);

                        dialog.Confirmed -= onConfirmed;
                        dialog.Close();
                        dialog.Dispose();
                        dialog = null;
                    };
                    dialog.Confirmed += onConfirmed;

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
