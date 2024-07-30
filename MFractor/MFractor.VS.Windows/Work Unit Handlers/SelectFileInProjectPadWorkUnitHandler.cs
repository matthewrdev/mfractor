using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;
using MFractor.VS.Windows.WorkspaceModel;
using MFractor.Work;
using MFractor.Ide.WorkUnits;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class SelectFileInProjectPadWorkUnitHandler : WorkUnitHandler<SelectFileInProjectPadWorkUnit>
    {
        readonly Lazy<IWorkspaceShadowModel> workspaceShadowModel;
        IWorkspaceShadowModel WorkspaceShadowModel => workspaceShadowModel.Value;

        [ImportingConstructor]
        public SelectFileInProjectPadWorkUnitHandler(Lazy<IWorkspaceShadowModel> workspaceShadowModel)
        {
            this.workspaceShadowModel = workspaceShadowModel;
        }

        public override async Task<IWorkExecutionResult> OnExecute(SelectFileInProjectPadWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var projectFile = WorkspaceShadowModel.GetProjectFile(workUnit.FilePath);

            if (projectFile != null)
            {
                Xwt.Application.InvokeAsync(() =>
               {

               }).ConfigureAwait(false);
            }

            Console.WriteLine(GetType().Name + "  is not implemented");

            return default(IWorkExecutionResult);
        }
    }
}
