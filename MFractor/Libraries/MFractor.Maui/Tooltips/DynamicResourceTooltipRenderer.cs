using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui.XamlPlatforms;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Tooltips
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDynamicResourceTooltipRenderer))]
    class DynamicResourceTooltipRenderer : IDynamicResourceTooltipRenderer
    {
        readonly Lazy<IDynamicResourceResolver> dynamicResourceResolver;
        public IDynamicResourceResolver DynamicResourceResolver => dynamicResourceResolver.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public DynamicResourceTooltipRenderer(Lazy<IDynamicResourceResolver> dynamicResourceResolver,
                                              Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                              Lazy<IProjectService> projectService)
        {
            this.dynamicResourceResolver = dynamicResourceResolver;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.projectService = projectService;
        }


        public string CreateTooltip(string dynamicResourceName,
                                    string currentXamlFile,
                                    Project project,
                                    IXamlPlatform platform)
        {
            var tooltip = "Dynamic Resource - " + dynamicResourceName;
            tooltip += Environment.NewLine;

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            var resources = DynamicResourceResolver.FindAvailableNamedDynamicResources(project, platform, currentXamlFile, dynamicResourceName);

            if (resources.Count > 1)
            {
                tooltip += "This resource is initialised " + resources.Count + " times.";
                tooltip += Environment.NewLine;
            }
            else
            {
                foreach (var r in resources)
                {
                    var resourceFile = database.GetRepository<ProjectFileRepository>().GetProjectFileById(r.Definition.ProjectFileKey);
                    var projectFile = ProjectService.GetProjectFileWithFilePath(project, resourceFile.FilePath);

                    if (projectFile != null)
                    {
                        var path = projectFile.ProjectFolders.Any() ? string.Join("/", projectFile.ProjectFolders) : "";

                        if (string.IsNullOrEmpty(path) == false)
                        {
                            path = Path.Combine(path, resourceFile.FileName);
                        }
                        else
                        {
                            path = resourceFile.FileName;
                        }

                        tooltip += "File: " + path;
                        tooltip += Environment.NewLine;
                    }

                    tooltip += "Return Type: " + r.Definition.ReturnType;

                    tooltip += Environment.NewLine;
                    tooltip += "Initialisation:";
                    tooltip += Environment.NewLine;
                    tooltip += r.Definition.Expression;
                }
            }

            return tooltip;
        }

        public string CreateTooltip(string dynamicResourceName,
                                    Project project)
        {
            var tooltip = "Dynamic Resource - " + dynamicResourceName;
            tooltip += Environment.NewLine;

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            var resources = DynamicResourceResolver.FindNamedDynamicResources(project, dynamicResourceName);

            if (resources.Count > 1)
            {
                tooltip += "This resource is initialised " + resources.Count + " times.";
                tooltip += Environment.NewLine;
            }
            else
            {
                foreach (var r in resources)
                {
                    var resourceFile = database.GetRepository<ProjectFileRepository>().GetProjectFileById(r.Definition.ProjectFileKey);
                    var projectFile = ProjectService.GetProjectFileWithFilePath(project, resourceFile.FilePath);

                    if (projectFile != null)
                    {
                        var path = projectFile.ProjectFolders.Any() ? string.Join("/", projectFile.ProjectFolders) : "";

                        if (!string.IsNullOrEmpty(path))
                        {
                            path = Path.Combine(path, resourceFile.FileName);
                        }
                        else
                        {
                            path = resourceFile.FileName;
                        }

                        tooltip += "File: " + path;
                        tooltip += Environment.NewLine;
                    }

                    tooltip += "Return Type: " + r.Definition.ReturnType;

                    tooltip += Environment.NewLine;
                    tooltip += "Initialisation:";
                    tooltip += Environment.NewLine;
                    tooltip += r.Definition.Expression;
                }
            }

            return tooltip;
        }
    }
}