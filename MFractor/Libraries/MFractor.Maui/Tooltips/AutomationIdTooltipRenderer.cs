using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Tooltips
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAutomationIdTooltipRenderer))]
    class AutomationIdTooltipRenderer : IAutomationIdTooltipRenderer
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public AutomationIdTooltipRenderer(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                           Lazy<IProjectService> projectService)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.projectService = projectService;
        }

        public string CreateTooltip(AutomationIdDeclaration automationId, Project project)
        {
            var tooltip = "Automation ID";

            tooltip += Environment.NewLine;
            tooltip += Environment.NewLine;
            tooltip += automationId.Name + " for " + automationId.ParentMetaDataName;

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            var file = database.GetRepository<ProjectFileRepository>().GetProjectFileById(automationId.ProjectFileKey);

            var projectFile = ProjectService.GetProjectFileWithFilePath(project, file.FilePath);

            if (projectFile != null)
            {
                var path = projectFile.ProjectFolders.Any() ? string.Join("/", projectFile.ProjectFolders) : "";

                if (string.IsNullOrEmpty(path) == false)
                {
                    path = Path.Combine(path, file.FileName);
                }
                else
                {
                    path = file.FileName;
                }

                tooltip += Environment.NewLine;
                tooltip += Environment.NewLine + "File: " + path;
                tooltip += Environment.NewLine + "Project: " + project.Name;
            }

            return tooltip;
        }
    }
}