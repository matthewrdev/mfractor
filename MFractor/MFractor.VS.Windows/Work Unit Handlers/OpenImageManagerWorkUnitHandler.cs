using System;
using System.Threading.Tasks;
using MFractor.Images.WorkUnits;
using MFractor.Progress;
using MFractor.VS.Windows.ToolWindows;
using MFractor.VS.Windows.Utilities;
using MFractor.Work;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class OpenImageManagerWorkUnitHandler : WorkUnitHandler<OpenImageManagerWorkUnit>
    {
        public override async Task<IWorkExecutionResult> OnExecute(OpenImageManagerWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Xwt.Application.InvokeAsync(() =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var imageManager = PackageHelper.FindToolWindow<ImageAssetsToolWindow>(0, true);
                if (imageManager == null || imageManager.Frame == null)
                {
                    throw new NotSupportedException("Cannot create Image Asset Manager Tool Window.");
                }

                var windowFrame = (IVsWindowFrame)imageManager.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());

                imageManager.SetSolution(workUnit.Solution);
                imageManager.SetOptions(workUnit.Options);
            });

            return default;
        }
    }
}
