using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Localisation.Data.Models;
using MFractor.Localisation.Data.Repositories;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Tooltips
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILocalisationTooltipRenderer))]
    class LocalisationTooltipRenderer : ILocalisationTooltipRenderer
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public LocalisationTooltipRenderer(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                          Lazy<IProjectService> projectService)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.projectService = projectService;
        }

        public string CreateLocalisationTooltip(ResXLocalisationDefinition definition, Project project)
        {
            if (definition is null
                || project is null)
            {
                return string.Empty;
            }

            var tooltip = string.Empty;

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            var entryRepository = database.GetRepository<ResXLocalisationEntryRepository>();
            var projectFileRepository = database.GetRepository<ProjectFileRepository>();

            var entries = entryRepository.GetEntriesForDefinition(definition);

            var first = true;
            foreach (var entry in entries.OrderBy(e => e.CultureCode))
            {
                if (!first)
                {
                    tooltip += Environment.NewLine;
                }

                first = false;
                tooltip += entry.CultureCode + ": " + entry.Value;
            }

            return tooltip;
        }

        public string CreateLocalisationTooltip(ILocalisationDeclarationCollection localisations)
        {
            if (localisations is null)
            {
                return string.Empty;
            }

            var tooltip = string.Empty;
            var first = true;
            foreach (var localisationDeclaration in localisations.OrderBy(e => e.CultureCode))
            {
                if (!first)
                {
                    tooltip += Environment.NewLine;
                }

                first = false;
                tooltip += localisationDeclaration.CultureCode + ": " + localisationDeclaration.Value;
            }

            return tooltip;
        }
    }
}