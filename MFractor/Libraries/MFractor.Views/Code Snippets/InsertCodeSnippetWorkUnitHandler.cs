using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.CodeSnippets
{
    class InsertCodeSnippetWorkUnitHandler : WorkUnitHandler<InsertCodeSnippetWorkUnit>
    {
        readonly Lazy<IRootWindowService> rootWindowService;
        public IRootWindowService RootWindowService => rootWindowService.Value;

        [ImportingConstructor]
        public InsertCodeSnippetWorkUnitHandler(Lazy<IRootWindowService> rootWindowService)
        {
            this.rootWindowService = rootWindowService;
        }

        public override async Task<IWorkExecutionResult> OnExecute(InsertCodeSnippetWorkUnit workUnit,
                                                                   IProgressMonitor progressMonitor)
        {
            var snippet = workUnit.CodeSnippet;

            var completionSource = new TaskCompletionSource<string>();

            var dialog = new InsertCodeSnippetDialog(snippet, workUnit.HelpUrl, workUnit.Title, workUnit.OnArgumentValueEditedFunc, workUnit.Title, workUnit.ConfirmButton);
            dialog.OnInsertCodeSnippet +=  (sender, args) =>
            {
                completionSource.TrySetResult(args.CodeSnippet.ToString());
            };

            dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
            dialog.Run(RootWindowService.RootWindowFrame);

            var insertion = string.Empty;
            try
            {
                insertion = await completionSource.Task;
            }
            catch (System.OperationCanceledException)
            {
                // Suppress.
            }

            if (!string.IsNullOrEmpty(insertion))
            {
                var result = new WorkExecutionResult();
                if (workUnit.ApplyCodeSnippetFunc != null)
                {
                    var workUnits = workUnit.ApplyCodeSnippetFunc(dialog.CodeSnippet);
                    result.AddPostProcessedWorkUnits(workUnits);
                }
                else
                {
                    var replacement = new Text.TextReplacement
                    {
                        Length = 0,
                        Text = insertion,
                        Offset = workUnit.InsertionOffset,
                        FilePath = workUnit.FilePath,
                        MoveCaretToReplacement = false
                    };

                    result.AddChangedFile(workUnit.FilePath);
                    result.AddAppliedWorkUnit(workUnit);
                    result.AddTextReplacement(replacement);
                }

                return result;
            }

            return default(WorkExecutionResult);
        }
    }
}
