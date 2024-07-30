using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Ide.WorkUnits;
using MFractor.Localisation.Data.Models;
using MFractor.Localisation.Data.Repositories;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILocalisationNavigationService))]
    class LocalisationNavigationService : ILocalisationNavigationService
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public LocalisationNavigationService(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public IReadOnlyList<IWorkUnit> Navigate(ResXLocalisationDefinition localisationDefinition, Project project)
        {
            if (localisationDefinition is null
                || project is null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database is null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var definitionRepository = database.GetRepository<ResXLocalisationEntryRepository>();
            var projectFileRepository = database.GetRepository<ProjectFileRepository>();

            var entries = definitionRepository.GetEntriesForDefinition(localisationDefinition);

            var workUnits = new List<NavigateToFileSpanWorkUnit>();
            foreach (var entry in entries)
            {
                var file = projectFileRepository.Get(entry.ProjectFileKey);
                if (file == null)
                {
                    continue;
                }

                var workUnit = new NavigateToFileSpanWorkUnit(entry.KeySpan, file.FilePath, project);

                workUnits.Add(workUnit);
            }

            return new NavigateToFileSpansWorkUnit(workUnits).AsList();
        }
    }
}