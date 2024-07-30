using System;
using System.ComponentModel.Composition;
using MFractor.Views;
using MonoDevelop.Ide;
using Xwt;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IRootWindowService))]
    class RootWindowService : IRootWindowService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public Xwt.WindowFrame RootWindowFrame
        {
            get
            {
                var window = IdeApp.Workbench.Window;
                try
                {
                    return Toolkit.CurrentEngine.WrapWindow(window);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
                return null;
            }
        }
    }
}