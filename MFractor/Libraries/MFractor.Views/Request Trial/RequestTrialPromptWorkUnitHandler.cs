using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Dialogs;
using MFractor.Progress;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Views;

namespace MFractor.Views.RequestTrial
{
    class RequestTrialPromptWorkUnitHandler : WorkUnitHandler<RequestTrialPromptWorkUnit>
    {
        const string requestTrialChoice = "Request Trial";
        const string upgradeChoice = "Upgrade To MFractor Professional";

        readonly Lazy<IDialogsService> dialogsService;
        IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IRootWindowService> rootWindowService;
        public IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public RequestTrialPromptWorkUnitHandler(Lazy<IDialogsService> dialogsService,
                                                 Lazy<IAnalyticsService> analyticsService,
                                                 Lazy<IWorkEngine> workEngine,
                                                 Lazy<IRootWindowService> rootWindowService,
                                                 Lazy<IUrlLauncher> urlLauncher,
                                                 Lazy<IDispatcher> dispatcher)
        {
            this.dialogsService = dialogsService;
            this.analyticsService = analyticsService;
            this.workEngine = workEngine;
            this.rootWindowService = rootWindowService;
            this.urlLauncher = urlLauncher;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(RequestTrialPromptWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);

                if (!string.IsNullOrEmpty(workUnit.FeatureName))
                {
                    AnalyticsService.Track(workUnit.FeatureName + " (Attempted Usage)");
                }

                if (workUnit.RequestTrialPromptMode == RequestTrialPromptMode.ToolbarOnly)
                {
                    DialogsService.ToolbarMessage(workUnit.Message);
                }
                else if (workUnit.RequestTrialPromptMode == RequestTrialPromptMode.PurchasePrompt)
                {
                    DialogsService.ToolbarMessage(workUnit.Message);

                    ShowPurchasePush(workUnit);
                }
                else if (workUnit.RequestTrialPromptMode == RequestTrialPromptMode.LaunchTrialDialog)
                {
                    RequestTrial();
                }

            }).ConfigureAwait(false);

            return null;
        }

        void ShowPurchasePush(RequestTrialPromptWorkUnit workUnit)
        {
            var message = workUnit.DetailedMessage;

            if (string.IsNullOrEmpty(message))
            {
                message = workUnit.Message;
            }

            var choice = DialogsService.AskQuestion("Please Upgrade\n" + message, requestTrialChoice, upgradeChoice, "Close");

            switch (choice)
            {
                case requestTrialChoice:
                    RequestTrial();
                    break;
                case upgradeChoice:
                    AnalyticsService.Track("Purchase Push Clicked");
                    UrlLauncher.OpenUrl("https://www.mfractor.com/buy");
                    break;
            }
        }

        void RequestTrial()
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               var dialog = new RequestTrialDialog();

               dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
               dialog.Run(RootWindowService.RootWindowFrame);
           });
        }
    }
}