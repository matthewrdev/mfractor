using System.Threading;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;

namespace MFractor.Work.WorkUnitHandlers
{
    class InsertTextWorkUnitHandler : WorkUnitHandler<InsertTextWorkUnit>
    {
        public override Task<IWorkExecutionResult> OnExecute(InsertTextWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var replaceChange = new Text.TextReplacement
            {
                Length = 0,
                Text = workUnit.Content,
                Offset = workUnit.Offset,
                FilePath = workUnit.FilePath,
                MoveCaretToReplacement = false
            };

            var result = new WorkExecutionResult();
            result.AddChangedFile(workUnit.FilePath);
            result.AddAppliedWorkUnit(workUnit);
            result.AddTextReplacement(replaceChange);

            return Task.FromResult<IWorkExecutionResult>(result);
        }
    }
}

