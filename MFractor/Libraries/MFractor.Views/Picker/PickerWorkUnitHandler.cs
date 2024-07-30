using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.Picker
{
    class PickerWorkUnitHandler : WorkUnitHandler<PickerWorkUnit>
    {
        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public PickerWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                     Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        public override async Task<IWorkExecutionResult> OnExecute(PickerWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var source = new TaskCompletionSource<object>();

            Dispatcher.InvokeOnMainThread(() =>
            {
                LaunchPickerDialog(workUnit, source);
            });

            object choice = null;
            try
            {
                choice = await source.Task;
            }
            catch
            {
            }

            var result = new WorkExecutionResult();

            if (choice == null)
            {
                return result;
            }

            if (workUnit.Delegate != null)
            {
                workUnit.Delegate.Invoke(choice);
            }

            return result;
        }

        void LaunchPickerDialog(PickerWorkUnit workUnit, TaskCompletionSource<object> source)
        {
            var dialog = new PickerDialog(workUnit.Choices, workUnit.Title, workUnit.Message, workUnit.Confirm, workUnit.Cancel, workUnit.HelpUrl);
            dialog.OnItemSelected += (sender, e) =>
            {
                source.SetResult(e.Item);
            };

            dialog.Closed += (sender, e) =>
            {
                source.TrySetCanceled();
            };

            dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
            dialog.Run(RootWindowService.RootWindowFrame);
        }
    }
}
