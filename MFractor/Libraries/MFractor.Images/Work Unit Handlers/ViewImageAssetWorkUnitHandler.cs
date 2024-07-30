using System.Linq;
using System.Threading.Tasks;
using MFractor.Images.ImageManager;
using MFractor.Images.WorkUnits;
using MFractor.Progress;
using MFractor.Work;

namespace MFractor.Images.WorkUnitHandlers
{
    class ViewImageAssetWorkUnitHandler : WorkUnitHandler<ViewImageAssetWorkUnit>
    {
        public override Task<IWorkExecutionResult> OnExecute(ViewImageAssetWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var result = new WorkExecutionResult();

            if (workUnit.ImageAsset != null)
            {
                result.AddPostProcessedWorkUnit(new OpenImageManagerWorkUnit()
                {
                    Solution = workUnit.ImageAsset.Projects.FirstOrDefault()?.Solution,
                    Force = workUnit.Force,
                    Options = ImageManagerOptions.Edit,
                    SelectedImageAsset = workUnit.ImageAsset.Name,
                });
            }

            return Task.FromResult<IWorkExecutionResult>(result);
        }
    }
}
