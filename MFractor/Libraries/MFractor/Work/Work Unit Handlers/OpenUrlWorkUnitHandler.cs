using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;
using MFractor;

namespace MFractor.Work.WorkUnitHandlers
{
    class OpenUrlWorkUnitHandler : WorkUnitHandler<OpenUrlWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IUrlLauncher> urlLauncher;
        public IUrlLauncher UrlLauncher => urlLauncher.Value;

        [ImportingConstructor]
        public OpenUrlWorkUnitHandler(Lazy<IUrlLauncher> urlLauncher)
        {
            this.urlLauncher = urlLauncher;
        }

        public override Task<IWorkExecutionResult> OnExecute(OpenUrlWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            try
            {
                UrlLauncher.OpenUrl(workUnit.Uri, workUnit.AddUtmSource);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return Task.FromResult<IWorkExecutionResult>(default);
        }
    }
}
