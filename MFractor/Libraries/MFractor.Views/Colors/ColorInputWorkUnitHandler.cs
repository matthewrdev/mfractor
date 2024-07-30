using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.Colors
{
    class ColorInputWorkUnitHandler : WorkUnitHandler<ColorEditorWorkUnit>
    {
        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public ColorInputWorkUnitHandler(Lazy<IRootWindowService> rootWindowService, Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(ColorEditorWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                var dialog = new ColorEditorDialog();
                dialog.Color = workUnit.Color;
                dialog.Confirmed += (sender, e) =>
                {
                    workUnit.ColorEditedDelegate(dialog.Color);
                    dialog.Close();
                    dialog.Dispose();
                    dialog = null;
                };

                dialog.Cancelled += (sender, e) =>
                {
                    dialog.Close();
                    dialog.Dispose();
                    dialog = null;
                };

                dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
                dialog.Run(RootWindowService.RootWindowFrame);
            });

            return default;
        }
    }
}
