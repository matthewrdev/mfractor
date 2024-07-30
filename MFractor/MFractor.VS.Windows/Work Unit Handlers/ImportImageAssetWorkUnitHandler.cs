using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.VS.Windows.UI.Dialogs;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class ImportImageAssetWorkUnitHandler : WorkUnitHandler<ImportImageAssetWorkUnit>
    {
        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public ImportImageAssetWorkUnitHandler(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(ImportImageAssetWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                var imageImporter = new ImageImporterDialog
                {
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                };
                imageImporter.ShowDialog();
            });

            return default;
        }
    }
}
