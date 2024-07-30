using System;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;
using System.ComponentModel.Composition;


namespace MFractor.Work.WorkUnitHandlers
{
    class StatusBarMessageWorkUnitHandler : WorkUnitHandler<StatusBarMessageWorkUnit>
    {
        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        [ImportingConstructor]
        public StatusBarMessageWorkUnitHandler(Lazy<IDialogsService> dialogsService)
        {
            this.dialogsService = dialogsService;
        }

        public override Task<IWorkExecutionResult> OnExecute(StatusBarMessageWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            DialogsService.StatusBarMessage(workUnit.Message);

            return default;
        }
    }
}
