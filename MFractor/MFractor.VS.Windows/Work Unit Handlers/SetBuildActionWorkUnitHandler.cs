using System;
using System.Composition;
using System.Threading.Tasks;
using MFractor.Code.WorkUnits;
using MFractor.IOC;
using MFractor.Progress;
using MFractor.VS.Windows.Utilities;
using MFractor.VS.Windows.WorkspaceModel;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class SetBuildActionWorkUnitHandler : WorkUnitHandler<SetBuildActionWorkUnit>
    {
        readonly Lazy<IDispatcher> dispatcher = new Lazy<IDispatcher>(Resolver.Resolve<IDispatcher>);
        IDispatcher Dispatcher => dispatcher.Value;

        readonly Lazy<IWorkspaceShadowModel> workspaceShadowModel = new Lazy<IWorkspaceShadowModel>(Resolver.Resolve<IWorkspaceShadowModel>);
        IWorkspaceShadowModel WorkspaceShadowModel => workspaceShadowModel.Value;


        public override async Task<IWorkExecutionResult> OnExecute(SetBuildActionWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            await Dispatcher.InvokeOnMainThreadAsync(async () =>
            {
                var projectFile = workUnit.ProjectFile;

                var ideProject = projectFile.CompilationProject.ToIdeProject();

                var projectFIle = WorkspaceShadowModel.GetProjectFile(workUnit.ProjectFile.FilePath);

                if (projectFIle is IdeProjectFile ideProjectFile)
                {
                    ideProjectFile.ProjectItem.Properties.Item("ItemType").Value = workUnit.BuildAction;
                    ideProjectFile.UpdateProperties();
                }
            });

            return default;
        }
    }
}
