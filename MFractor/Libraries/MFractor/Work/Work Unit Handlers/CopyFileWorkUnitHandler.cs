using System.IO;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;

namespace MFractor.Work.WorkUnitHandlers
{
    class CopyFileWorkUnitHandler : WorkUnitHandler<CopyFileWorkUnit>
    {
        public override Task<IWorkExecutionResult> OnExecute(CopyFileWorkUnit workUnit, IProgressMonitor progressMonitor)
        { 
            File.Copy(workUnit.SourceFilePath, workUnit.DestinationFilePath);

            return Task.FromResult<IWorkExecutionResult>(default);
        }
    }
}
