using System.Threading;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;

namespace MFractor.Work.WorkUnitHandlers
{
    class ReplaceTextWorkUnitHandler : WorkUnitHandler<ReplaceTextWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public override Task<IWorkExecutionResult> OnExecute(ReplaceTextWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            if (string.IsNullOrEmpty(workUnit.FilePath))
            {
                log?.Warning("No file path provided to workUnit handler");
                return default;
            }

            var replaceChange = new Text.TextReplacement
            {
                Length = workUnit.Span.Length,
                Text = workUnit.Text,
                Description = "",
                Offset = workUnit.Span.Start,
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
