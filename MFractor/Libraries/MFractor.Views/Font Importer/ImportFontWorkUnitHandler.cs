using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Fonts.WorkUnits;
using MFractor.Progress;
using MFractor.Work;

namespace MFractor.Views.FontImporter
{
    class ImportFontWorkUnitHandler : WorkUnitHandler<ImportFontWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ImportFontWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                         Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override Task<IWorkExecutionResult> OnExecute(ImportFontWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            return Dispatcher.InvokeOnMainThreadAsync<IWorkExecutionResult>(() =>
            {
                var fontImporter = new FontImporterDialog(workUnit.Solution, workUnit.ProjectIdentifier);
                fontImporter.FontImported += (sender, e) =>
                {
                    if (workUnit != null && workUnit.ImportAction != null)
                    {
                        try
                        {
                            workUnit.ImportAction(e.FontImportResult);
                        }
                        catch (Exception ex)
                        {
                            log?.Exception(ex);
                        }
                    }
                };

                fontImporter.InitialLocation = Xwt.WindowLocation.CenterParent;
                fontImporter.Run(RootWindowService.RootWindowFrame);

                return default;
            });
        }
    }
}
