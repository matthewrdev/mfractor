using System;
using System.ComponentModel.Composition;
using MFractor.Android.Data.Repositories.Manifest;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Ide.Commands;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using Microsoft.CodeAnalysis;

namespace MFractor.Android.Commands
{
    [RequiresLicense]
    [Export]
    class CopyPackageNameToClipboardCommand : ICommand
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public CopyPackageNameToClipboardCommand(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                                 Lazy<IProjectService> projectService,
                                                 Lazy<IWorkEngine> workEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.projectService = projectService;
            this.workEngine = workEngine;
        }

        Project GetTargetProject(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is Project project
                && project.IsAndroidProject())
            {
                return project;
            }

            return default;
        }

        public void Execute(ICommandContext commandContext)
        {
            var project = GetTargetProject(commandContext);

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
            var repo = database.GetRepository<PackageDetailsRepository>();

            var details = repo.GetPackageDetails();

            WorkEngine.ApplyAsync(new CopyValueToClipboardWorkUnit()
            {
                Value = details.PackageName,
                Message = $"Copied {details.PackageName} to clipboard"
            }).ConfigureAwait(false);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var project = GetTargetProject(commandContext);

            if (project == null)
            {
                return default;
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
            if (database == null || !database.IsValid)
            {
                return default;
            }

            return new CommandState("Copy Package Id To Clipboard", "Copy the package id to the clipboard");
        }
    }
}