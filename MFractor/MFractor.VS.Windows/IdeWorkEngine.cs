using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using MFractor.Progress;
using MFractor.Text;
using MFractor.VS.Windows.Utilities;
using MFractor.Work;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace MFractor.VS.Windows
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IWorkEngine))]
    class IdeWorkEngine : WorkEngine
    {
        readonly Lazy<IDispatcher> dispatcher;
        IDispatcher Dispatcher => dispatcher.Value;

        private DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        [ImportingConstructor]
        public IdeWorkEngine(Lazy<IWorkUnitHandlerRepository> workUnitHandlerRepository,
                             Lazy<ITextReplacementService> textReplacementService,
                             Lazy<IDispatcher> dispatcher)
            : base(workUnitHandlerRepository, textReplacementService)
        {
            this.dispatcher = dispatcher;
        }

        protected override async Task ApplyChanges(IReadOnlyList<ProjectIdentifier> projectsToSave, 
                                                   IReadOnlyList<FileCreation> filesToCreate, 
                                                   IProgressMonitor progressMonitor)
        {
            foreach (var file in filesToCreate)
            {
                DTE.ItemOperations.OpenFile(file.FilePath);
            }

            foreach (var pid in projectsToSave)
            {
                var project = pid.ToIdeProject();

                if (project != null)
                {
                    project.Save();
                }
            }
        }

        protected override async Task OpenCreatedFiles(WorkExecutionResult workUnitResult)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                var createdFiles = workUnitResult.FilesToCreate.ToList();

                // TODO: For each of the createdFiles, open them in the editor.
            });
        }

        protected override void NotifyChangedFiles(WorkExecutionResult workUnitResult)
        {
            var changed = workUnitResult.ChangedFiles;

            // TODO: For all changed, send a file change message.
        }
    }
}
