using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Text;
using MFractor.VS.Mac.Progress;
using MFractor.VS.Mac.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Refactoring;
using MFractor;
using MFractor.Workspace.WorkUnits;

namespace MFractor.VS.Mac
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IWorkEngine))]
    class IdeWorkEngine : WorkEngine
    {
        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

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
            if (filesToCreate != null && filesToCreate.Any())
            {
                // TODO: Create files on disk.
                // TODO: IdeApp.ProjectOperations.AddFilesToProject();
            }

            if (projectsToSave != null && projectsToSave.Any())
            {
                foreach (var projectId in projectsToSave)
                {
                    var ideProject = projectId.ToIdeProject();
                    if (ideProject != null)
                    {
                        await IdeApp.ProjectOperations.SaveAsync(ideProject);
                    }
                }
            }
        }

        protected override async Task OpenCreatedFiles(WorkExecutionResult result)
        {
            await Dispatcher.InvokeOnMainThreadAsync(() =>
            {
                var createdFiles = result.FilesToCreate.ToList();
                if (createdFiles.Count > 0)
                {
                    if (result.AppliedWorkUnits != null)
                    {
                        var excludedFiles = result.AppliedWorkUnits.OfType<CreateProjectFileWorkUnit>().Where(r => r.ShouldOpen == false);

                        foreach (var e in excludedFiles)
                        {
                            var match = createdFiles.FirstOrDefault((cf) =>
                            {
                                if (string.IsNullOrEmpty(cf.FilePath))
                                {
                                    return false;
                                }

                                if (string.IsNullOrEmpty(e.FilePath))
                                {
                                    return false;
                                }

                                return cf.FilePath.EndsWith(e.FilePath, StringComparison.Ordinal);
                            });

                            if (match != null)
                            {
                                createdFiles.Remove(match);
                            }
                        }
                    }

                    IdeApp.OpenFiles(createdFiles.Select((arg) => new FileOpenInformation(arg.FilePath)));
                }
            });
        }

        protected override void NotifyChangedFiles(WorkExecutionResult result)
        {
            var changed = result.ChangedFiles.Select(f => new FilePath(f));
            var toNotify = new List<FilePath>();

            foreach (var file in changed)
            {
                var opened = IdeApp.Workbench.Documents.Any(d => d.FileName == file);
                if (!opened)
                {
                    toNotify.Add(file);
                }
            }

            FileService.NotifyFilesChanged(toNotify, false);
        }
    }
}
