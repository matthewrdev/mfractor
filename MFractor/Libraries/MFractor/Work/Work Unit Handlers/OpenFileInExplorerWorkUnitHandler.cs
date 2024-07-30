using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;

namespace MFractor.Work.WorkUnitHandlers
{
    class OpenFileInExplorerWorkUnitHandler : WorkUnitHandler<OpenFileInExplorerWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IOpenFileInBrowserService> openFileInBrowserService;
        public IOpenFileInBrowserService OpenFileInBrowserService => openFileInBrowserService.Value;

        [ImportingConstructor]
        public OpenFileInExplorerWorkUnitHandler(Lazy<IOpenFileInBrowserService> openFileInBrowserService)
        {
            this.openFileInBrowserService = openFileInBrowserService;
        }

        public override Task<IWorkExecutionResult> OnExecute(OpenFileInExplorerWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            try
            {
                OpenFileInBrowserService.OpenAndSelect(workUnit.FilePath);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return Task.FromResult<IWorkExecutionResult>(default);
        }
    }
}
