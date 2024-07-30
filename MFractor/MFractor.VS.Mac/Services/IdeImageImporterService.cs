using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

using MFractor.Images;
using MFractor.Images.Importing;
using MFractor.Progress;
using MFractor.VS.Mac.Utilities;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageImporterService))]
    class IdeImageImporterService : ImageImporterService
    {
        [ImportingConstructor]
        public IdeImageImporterService(Lazy<IDialogsService> dialogsService,
                                       Lazy<IImageUtilities> imageUtil)
            : base(dialogsService, imageUtil)
        {
        }

        protected override Task<bool> AddProjectFiles(ImportImageOperation operation, Project project, IEnumerable<CreateProjectFileWorkUnit> workUnits, IProgressMonitor progressMonitor)
        {
            return Xwt.Application.InvokeAsync(() =>
            {
                var ideProject = project.ToIdeProject();

                foreach (var workUnit in workUnits)
                {
                    var diskPath = workUnit.FilePath;

                    var existingFile = ideProject.GetProjectFile(diskPath);

                    if (existingFile != null)
                    {
                        continue;
                    }

                    var file = workUnit.HasBuildAction ? ideProject.AddFile(diskPath, workUnit.BuildAction) : ideProject.AddFile(diskPath);
                    file.Visible = workUnit.Visible;

                    if (!string.IsNullOrEmpty(workUnit.BuildAction))
                    {
                        file.BuildAction = workUnit.BuildAction;
                    }
                }

                ideProject.SaveAsync(new MonoDevelop.Core.ProgressMonitor()).ConfigureAwait(false);

                return true;
            });
        }
    }
}