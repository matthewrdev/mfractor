using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;
using MFractor.Maui.StaticResources;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Tooltips
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IStaticResourceTooltipRenderer))]
    class StaticResourceTooltipRenderer : IStaticResourceTooltipRenderer
    {
        readonly Lazy<IStaticResourceResolver> staticResourceResolver;
        public IStaticResourceResolver StaticResourceResolver => staticResourceResolver.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public StaticResourceTooltipRenderer(Lazy<IStaticResourceResolver> staticResourceResolver,
                                             Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                             Lazy<IProjectService> projectService)
        {
            this.staticResourceResolver = staticResourceResolver;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.projectService = projectService;
        }

        public string CreateTooltip(StaticResourceDefinition definition, Project project, bool includeXmlPreview = true)
        {
            var tooltip = "Static Resource - " + definition.Name + " (" + definition.SymbolMetaType + ")";

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            var file = database.GetRepository<ProjectFileRepository>().GetProjectFileById(definition.ProjectFileKey);

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
                tooltip += Environment.NewLine + "File: " + path + " in " + project.Name;
            }

            try
            {
                var content = definition.PreviewString;
                if (!string.IsNullOrEmpty(content) && includeXmlPreview)
                {
                    var preview = XmlFormattingHelper.FormatXml(content);

                    if (!string.IsNullOrEmpty(preview))
                    {
                        tooltip += "\n\n" + preview;
                    }
                }
            }
            catch (Exception)
            {
            }

            return tooltip;
        }
    }
}