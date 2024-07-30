using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MonoDevelop.Ide;
using MFractor;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class OpenPreferencesWorkUnitHandler : WorkUnitHandler<OpenPreferencesWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public OpenPreferencesWorkUnitHandler(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(OpenPreferencesWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               try
               {
                   var id = "MFractor.Settings";

                   if (!string.IsNullOrEmpty(workUnit.PreferencesId))
                   {
                       id = workUnit.PreferencesId;
                   }

                   var window = IdeServices.DesktopService.GetFocusedTopLevelWindow();
                   IdeApp.Workbench.ShowGlobalPreferencesDialog(window, id);
               }
               catch (Exception ex)
               {
                   log?.Exception(ex);
               }
           });

            return Task.FromResult(default(IWorkExecutionResult));
        }
    }
}
