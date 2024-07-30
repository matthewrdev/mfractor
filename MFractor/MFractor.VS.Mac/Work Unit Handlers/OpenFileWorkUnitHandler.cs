using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class OpenFileWorkUnitHandler : WorkUnitHandler<OpenFileWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public OpenFileWorkUnitHandler(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(OpenFileWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var project = workUnit.Project?.ToIdeProject() ?? IdeApp.Workbench.ActiveDocument?.Owner as MonoDevelop.Projects.Project;

            Dispatcher.InvokeOnMainThread(async () =>
            {
                await IdeApp.Workbench.OpenDocument(workUnit.FilePath, project, OpenDocumentOptions.BringToFront | OpenDocumentOptions.TryToReuseViewer | OpenDocumentOptions.HighlightCaretLine);

                await ExecuteOnDocumentOpenedAction(workUnit);
            });

            return default;
        }

        Task ExecuteOnDocumentOpenedAction(OpenFileWorkUnit workUnit)
        {
            return Task.Run(async () =>
            {
                var count = 0;
                while (IdeApp.Workbench.ActiveDocument == null || IdeApp.Workbench.ActiveDocument.FilePath != workUnit.FilePath)
                {
                    if (count > 25)
                    {
                        return;
                    }

                    count++;
                    await Task.Delay(100);
                }

                try
                {
                    workUnit?.OnDocumentOpenedAction();
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);

                }
            });
        }
    }
}
