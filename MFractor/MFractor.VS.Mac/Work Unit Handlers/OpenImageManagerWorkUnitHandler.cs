using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Images.WorkUnits;
using MFractor.Progress;
using MFractor.Views.ImageManager;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class OpenImageManagerWorkUnitHandler : WorkUnitHandler<OpenImageManagerWorkUnit>
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public OpenImageManagerWorkUnitHandler(Lazy<IWorkspaceService> workspaceService,
                                              Lazy<IDispatcher> dispatcher)
        {
            this.workspaceService = workspaceService;
            this.dispatcher = dispatcher;
        }

        public override Task<IWorkExecutionResult> OnExecute(OpenImageManagerWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var solution = workUnit.Solution ?? WorkspaceService.CurrentWorkspace.CurrentSolution;

            Dispatcher.InvokeOnMainThread(() =>
            {
                var imageManager = ImageManagerDialog.GetOrCreate(workUnit.Solution, workUnit.Options);

                if (!string.IsNullOrEmpty(workUnit.SelectedImageAsset))
                {
                    imageManager.Select(workUnit.SelectedImageAsset);
                }
            });


            return Task.FromResult<IWorkExecutionResult>(default);
        }
    }
}
