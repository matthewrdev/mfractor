using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Views;
using MFractor.Views.AppIconImporter;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class ImportIconWizardWorkUnitHandler : WorkUnitHandler<ImportIconWizardWorkUnit>
    {
        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public ImportIconWizardWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                               Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(ImportIconWizardWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                var window = new AppIconImporterDialog(workUnit.Projects);
                window.Run(RootWindowService.RootWindowFrame);
            });

            return Task.FromResult<IWorkExecutionResult>(default);
        }
    }
}
