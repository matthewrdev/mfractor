using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Ide.DeleteOutputFolders;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.DeleteOutputFolders
{
    class DeleteOutputFolderWorkUnitHandler : WorkUnitHandler<DeleteOutputFoldersWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IRootWindowService> rootWindowService;
        public IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDeleteOutputFoldersService> deleteOutputFoldersService;
        public IDeleteOutputFoldersService DeleteOutputFoldersService => deleteOutputFoldersService.Value;

        readonly Lazy<IDeleteOutputFoldersConfigurationService> deleteOutputFoldersConfigurationService;
        public IDeleteOutputFoldersConfigurationService DeleteOutputFoldersConfigurationService => deleteOutputFoldersConfigurationService.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        [ImportingConstructor]
        public DeleteOutputFolderWorkUnitHandler(Lazy<IRootWindowService> rootWindowService,
                                                 Lazy<IDeleteOutputFoldersService> deleteOutputFoldersService,
                                                 Lazy<IDeleteOutputFoldersConfigurationService> deleteOutputFoldersConfigurationService,
                                                 Lazy<IDialogsService> dialogsService)
        {
            this.rootWindowService = rootWindowService;
            this.deleteOutputFoldersService = deleteOutputFoldersService;
            this.deleteOutputFoldersConfigurationService = deleteOutputFoldersConfigurationService;
            this.dialogsService = dialogsService;
        }

        public override Task<IWorkExecutionResult> OnExecute(DeleteOutputFoldersWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var hasConfiguration = false;
            if (workUnit.Project != null)
            {
                hasConfiguration = DeleteOutputFoldersConfigurationService.HasConfiguration(workUnit.Project);
            }
            else if (workUnit.Solution != null)
            {
                hasConfiguration = DeleteOutputFoldersConfigurationService.HasConfiguration(workUnit.Solution);
            }
            else
            {
                log?.Warning("No project or solution was provided to the delete output folders feature.");
                return Task.FromResult<IWorkExecutionResult>(default);
            }

            if (!hasConfiguration)
            {
                var dialog = workUnit.Project != null ? new DeleteOutputFoldersConfigurationDialog(workUnit.Project) : new DeleteOutputFoldersConfigurationDialog(workUnit.Solution);
                dialog.DeleteOutputFoldersConfigurationConfirmedDelegate = () =>
                {
                    DeleteOutputFolders(workUnit);
                };

                dialog.Run(RootWindowService.RootWindowFrame);
            }
            else
            {
                DeleteOutputFolders(workUnit);
            }

            return Task.FromResult<IWorkExecutionResult>(default);
        }

        void DeleteOutputFolders(DeleteOutputFoldersWorkUnit workUnit)
        {
            if (workUnit.Project != null)
            {
                var options = DeleteOutputFoldersConfigurationService.GetOptionsOrDefault(workUnit.Project);

                using (var task = DialogsService.StartStatusBarTask("Deleting output folders..."))
                {
                    DeleteOutputFoldersService.DeleteOutputFolders(workUnit.Project, options, task);
                }
            }
            else if (workUnit.Solution != null)
            {
                var options = DeleteOutputFoldersConfigurationService.GetOptionsOrDefault(workUnit.Solution);

                using (var task = DialogsService.StartStatusBarTask("Deleting output folders..."))
                {
                    DeleteOutputFoldersService.DeleteOutputFolders(workUnit.Solution, options, true, task);
                }
            }
            else
            {
                log?.Warning("No project or solution was provided to the delete output folders feature.");
                return;
            }
        }
    }
}
