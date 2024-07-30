using System;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.AppIconImporter
{
    class AppIconImporterWorkUnitHandler : WorkUnitHandler<AppIconImporterWorkUnit>
    {
        public override Task<IWorkExecutionResult> OnExecute(AppIconImporterWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            // Handle the workUnit and display the app icon wizard.

            return default;
        }
    }
}
