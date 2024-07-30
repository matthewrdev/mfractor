using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class NavigateToFileSpanWorkUnitHandler : WorkUnitHandler<NavigateToFileSpanWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public NavigateToFileSpanWorkUnitHandler(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        readonly OpenDocumentOptions options = OpenDocumentOptions.BringToFront | OpenDocumentOptions.TryToReuseViewer | OpenDocumentOptions.HighlightCaretLine;

        public override async Task<IWorkExecutionResult> OnExecute(NavigateToFileSpanWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(async () =>
            {
                var currentDocument = IdeApp.Workbench.ActiveDocument;
                try
                {
                    if (currentDocument != null && currentDocument.Name == workUnit.FilePath)
                    {
                        SetCaretLocation(currentDocument, workUnit);
                    }
                    else
                    {
                        var monoDevelopProject = workUnit.Project?.ToIdeProject();
                        var openInfo = new FileOpenInformation(workUnit.FilePath, monoDevelopProject);
                        openInfo.Options = options;
                        openInfo.Offset = workUnit.Span.Start;
                        var doc = await IdeApp.Workbench.OpenDocument(openInfo);

                        SetCaretLocation(doc, workUnit);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            });

            return default(WorkExecutionResult);
        }

        static void SetCaretLocation(Document currentDocument, NavigateToFileSpanWorkUnit workUnit)
        {
           if (workUnit.Span.Length > 1)
            {
                currentDocument.SetSelection(workUnit.Span.Start, workUnit.Span.End);
            }
            else
            {
                currentDocument.SetCaretLocation(workUnit.Span.Start);
            }
        }
    }
}
