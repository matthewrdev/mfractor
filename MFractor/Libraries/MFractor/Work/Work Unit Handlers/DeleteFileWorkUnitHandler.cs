using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

using MFractor.Progress;
using MFractor.Work.WorkUnits;

namespace MFractor.Work.WorkUnitHandlers
{
    class DeleteProjectFileWorkUnitHandler : WorkUnitHandler<DeleteFileWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        [ImportingConstructor]
        public DeleteProjectFileWorkUnitHandler(Lazy<IDialogsService> dialogsService)
        {
            this.dialogsService = dialogsService;
        }

        public override Task<IWorkExecutionResult> OnExecute(DeleteFileWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            if (!File.Exists(workUnit.FilePath))
            {
                log?.Warning($"Cannot delete {workUnit.FilePath} as the file does not exist");
                return Task.FromResult(default(IWorkExecutionResult));
            }

            File.Delete(workUnit.FilePath);

            return Task.FromResult(default(IWorkExecutionResult));
        }
    }
}
