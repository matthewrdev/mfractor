using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Images.WorkUnits;
using MFractor.Progress;
using MFractor.Work;

namespace MFractor.Views.ImageDeletionTool
{
    class DeleteImageAssetWorkUnitHandler : WorkUnitHandler<DeleteImageAssetWorkUnit>
    {
        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public DeleteImageAssetWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                               Lazy<IDispatcher> dispatcher)
        {
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(DeleteImageAssetWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               var dialog = new DeleteImagesDialog(workUnit.ImageAsset);

               dialog.ImagesDeleted += (sender, e) =>
               {
                   workUnit.OnImagesDeleted?.Invoke(e.DeletedFiles);
               };

               dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
               dialog.Run(RootWindowService.RootWindowFrame);
           });

            return default;
        }
    }
}
