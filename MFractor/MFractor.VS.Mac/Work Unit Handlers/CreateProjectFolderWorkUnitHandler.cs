using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class CreateProjectFolderWorkUnitHandler : WorkUnitHandler<CreateProjectFolderWorkUnit>
    {
        public override async Task<IWorkExecutionResult> OnExecute(CreateProjectFolderWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Xwt.Application.InvokeAsync(() =>
            {
                var ideProject = workUnit.Project.ToIdeProject();
                ideProject.AddDirectory(workUnit.VirtualPath);
            });

            return default;
        }
    }
}
