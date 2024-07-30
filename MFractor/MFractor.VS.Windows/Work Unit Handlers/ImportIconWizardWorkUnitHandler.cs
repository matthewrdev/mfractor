using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Views;
using MFractor.Views.AppIconImporter;
using MFractor.VS.Windows.UI.Dialogs;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class ImportIconWizardWorkUnitHandler : WorkUnitHandler<ImportIconWizardWorkUnit>
    {
        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public ImportIconWizardWorkUnitHandler(Lazy<IDispatcher> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(ImportIconWizardWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                var imageImporter = new IconImporterDialog(workUnit.Projects)
                {
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                };
                imageImporter.ShowDialog();
            });

            return default;
        }

    }
}
