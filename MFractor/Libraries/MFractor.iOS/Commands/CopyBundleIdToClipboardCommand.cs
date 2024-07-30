using System;
using System.ComponentModel.Composition;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Ide.Commands;
using MFractor.iOS.Data.Repositories;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using Microsoft.CodeAnalysis;

namespace MFractor.iOS.Commands
{
    [RequiresLicense]
    [Export]
    public class CopyBundleIdToClipboardCommand : ICommand
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngineAccessor;
        readonly Lazy<IProjectService> projectServiceAccessor;
        readonly Lazy<IWorkEngine> workEngineAccessor;

        IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngineAccessor.Value;
        IProjectService ProjectService => projectServiceAccessor.Value;
        IWorkEngine WorkEngine => workEngineAccessor.Value;

        [ImportingConstructor]
        public CopyBundleIdToClipboardCommand(
            Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
            Lazy<IProjectService> projectService,
            Lazy<IWorkEngine> workEngine)
        {
            resourcesDatabaseEngineAccessor = resourcesDatabaseEngine;
            projectServiceAccessor = projectService;
            workEngineAccessor = workEngine;
        }

        public void Execute(ICommandContext commandContext)
        {
            var project = GetTargetProject(commandContext);
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
            var repo = database.GetRepository<BundleDetailsRepository>();
            var details = repo.GetBundleDetails();

            WorkEngine.ApplyAsync(new CopyValueToClipboardWorkUnit
            {
                Value = details.BundleIdentifier,
                Message = $"Copied {details.BundleIdentifier} to clipboard",
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

            return new CommandState("Copy Bundle Id To Clipboard", "Copies the iOS Bundle Identifier to the clipboard.");
        }

        Project GetTargetProject(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext
                && solutionPadCommandContext.SelectedItem is Project project
                && project.IsIOSProject())
            {
                return project;
            }

            return default;
        }
    }
}
