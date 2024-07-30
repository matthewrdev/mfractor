using System.Threading.Tasks;
using MFractor.Code.WorkUnits;
using MFractor.Progress;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class SetBuildActionWorkUnitHandler : WorkUnitHandler<SetBuildActionWorkUnit>
    {
        public override async Task<IWorkExecutionResult> OnExecute(SetBuildActionWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Xwt.Application.InvokeAsync(async () =>
            {
                var projectFile = workUnit.ProjectFile;

                var ideProject = projectFile.CompilationProject.ToIdeProject();

                var ideProjectFile = ideProject.GetProjectFile(projectFile.FilePath);

                ideProjectFile.BuildAction = workUnit.BuildAction;

                await ideProject.SaveAsync(IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor("Saving " + ideProject.Name, null)).ConfigureAwait(false);
            });

            return default;
        }
    }
}
