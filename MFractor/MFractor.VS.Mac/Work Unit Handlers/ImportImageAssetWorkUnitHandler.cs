using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Views;
using MFractor.Views.ImageImporter;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.VS.Mac.WorkUnitHandlers
{
    class ImportImageAssetWorkUnitHandler : WorkUnitHandler<ImportImageAssetWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ImportImageAssetWorkUnitHandler(Lazy<IWorkEngine> workUnitEngine,
                                               Lazy<IRootWindowService> rootWindowService,
                                               Lazy<IWorkspaceService> workspaceService)
        {
            this.workUnitEngine = workUnitEngine;
            this.rootWindowService = rootWindowService;
            this.workspaceService = workspaceService;
        }

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IWorkEngine> workUnitEngine;
        public IWorkEngine  WorkUnitEngine => workUnitEngine.Value;

        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        public override Task<IWorkExecutionResult> OnExecute(ImportImageAssetWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var projects = workUnit.Projects ?? WorkspaceService.CurrentWorkspace.CurrentSolution.GetMobileProjects().Where(p => !p.IsUWPProject());

            if (!projects.Any())
            {
                log?.Warning("No projects provided when executing the image importer");
            }

            var window = new ImageImporterDialog(projects.Select(p => new ProjectSelection(p, true)).ToList(), workUnit.ImageName);

            void onImageImported(object sender, ImportImageEventArgs args)
            {
                if (workUnit.OnImageNameConfirmedCallback != null && !string.IsNullOrEmpty(args.ImageName))
                {
                    var workUnits = workUnit.OnImageNameConfirmedCallback(args.ImageName);

                    WorkUnitEngine.ApplyAsync(workUnits).ConfigureAwait(false);
                }

                window.OnImageImported -= onImageImported;
            }

            window.AllowMultipleImports = workUnit.AllowMultipleImports;
            window.OnImageImported += onImageImported;
            window.InitialLocation = Xwt.WindowLocation.CenterParent;

            if (!string.IsNullOrEmpty(workUnit.ImageFilePath) && File.Exists(workUnit.ImageFilePath))
            {
                window.SetImage(workUnit.ImageFilePath, true);
            }

            window.Run(RootWindowService.RootWindowFrame);

            return Task.FromResult<IWorkExecutionResult>(default);
        }
    }
}
