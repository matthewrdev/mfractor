using System;
using System.ComponentModel.Composition;
using EnvDTE;
using EnvDTE80;
using MFractor.Ide;
using MFractor.Ide.WorkUnitHandlers;
using MFractor.VS.Windows.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace MFractor.VS.Windows.WorkUnitHandlers
{
    class IdeCreateProjectFileWorkUnitHandler : BaseCreateProjectFileWorkUnitHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        private DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        [ImportingConstructor]
        public IdeCreateProjectFileWorkUnitHandler(Lazy<IFileCreationPostProcessorRepository> fileCreationPostProcessors, Lazy<IDialogsService> dialogsService)
            : base(fileCreationPostProcessors, dialogsService)
        {
        }

        protected override void AddMSBuildEntry(string diskPath, Microsoft.CodeAnalysis.Project project, CreateProjectFileWorkUnit workUnit)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var ideProject = project.ToIdeProject();
            if (ideProject != null)
            {
                var newFile = ideProject.ProjectItems.AddFromFile(diskPath);

                if (!string.IsNullOrEmpty(workUnit.BuildAction))
                {
                    newFile.Properties.Item("ItemType").Value = workUnit.BuildAction;
                }

                if (workUnit.HasDependsUponFile)
                {
                    newFile.Properties.Item("DependentUpon").Value = workUnit.DependsUponFile;
                }
            }
        }

        protected override async Task AddVirtualDirectory(Microsoft.CodeAnalysis.Project project, string virtualDirectoryPath)
        {
            // TODO:
        }
    }
}
