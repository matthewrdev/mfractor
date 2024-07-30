
using System;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MFractor.VS.Windows.Utilities
{
    class PackageHelper
    {
        internal static MFractorPackage GetPackage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var vsShell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));

            Assumes.Present(vsShell);

            var packageGuid = new Guid(MFractorPackage.PackageGuidString);
            if (vsShell.IsPackageLoaded(ref packageGuid, out var myPackage)  == Microsoft.VisualStudio.VSConstants.S_OK)
            {
                return (MFractorPackage)myPackage;
            }

            return default;
        }

        internal static void ShowOptionPage(Type type)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            GetPackage()?.ShowOptionPage(type);
        }

        internal static TToolWindowPane FindToolWindow<TToolWindowPane>(int id, bool create) where TToolWindowPane : ToolWindowPane
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return FindToolWindow(typeof(TToolWindowPane), id, create) as TToolWindowPane;
        }

        internal static ToolWindowPane FindToolWindow(Type type, int id, bool create)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return GetPackage()?.FindToolWindow(type, id, create);
        }
    }
}
