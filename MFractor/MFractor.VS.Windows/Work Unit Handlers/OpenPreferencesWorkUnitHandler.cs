using System;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;
using MFractor.VS.Windows.UI.Options;
using MFractor.VS.Windows.Utilities;
using Microsoft.VisualStudio.Shell;
using MFractor.Work;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class OpenPreferencesWorkUnitHandler : WorkUnitHandler<OpenPreferencesWorkUnit>
    {
        public override async Task<IWorkExecutionResult> OnExecute(OpenPreferencesWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Xwt.Application.InvokeAsync(() =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                PackageHelper.ShowOptionPage(typeof(SettingsPage));
            });

            return default;
        }
    }
}
