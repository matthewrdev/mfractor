using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.TextInput
{
    class TextInputWorkUnitHandler : WorkUnitHandler<TextInputWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public TextInputWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                        Lazy<IDialogsService> dialogsService)
        {
            this.rootWindowService = rootWindowService;
            this.dialogsService = dialogsService;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        IRootWindowService RootWindowService => rootWindowService.Value;

        public async override Task<IWorkExecutionResult> OnExecute(TextInputWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var dialog = new TextInputDialog(workUnit.Title, workUnit.Message, workUnit.Value, workUnit.Confirm, workUnit.Cancel);

            var completionSource = new TaskCompletionSource<string>();
            dialog.OnEntryConfirmed += (sender, args) =>
            {
                try
                {
                    if (workUnit.ValidateTextInputDelegate != null && !workUnit.ValidateTextInputDelegate(args.Value, out var message))
                    {
                        DialogsService.ShowMessage(message, string.Empty);
                        return;
                    }

                    completionSource.TrySetResult(args.Value);
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            };

            dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
            dialog.Run(RootWindowService.RootWindowFrame);

            var value = await completionSource.Task;

            var workUnits = await Task.Run(() =>
            {
                return workUnit.TextInputResultDelegate(value);
            });

            var result = new WorkExecutionResult();
            result.AddPostProcessedWorkUnits(workUnits);

            return result;
        }
    }
}
