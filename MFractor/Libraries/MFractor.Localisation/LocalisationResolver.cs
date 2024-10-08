using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Localisation.Data.Repositories;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILocalisationResolver))]
    class LocalisationResolver : ILocalisationResolver
    {
        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IProjectService> projectService;        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public LocalisationResolver(Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                    Lazy<IProjectService> projectService)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.projectService = projectService;
        }

        public ILocalisationDeclarationCollection ResolveLocalisations(Project project, string key)
        {
            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);
            if (database is null  || !database.IsValid)
            {
                return null;
            }

            var resXDefinitinRepo = database.GetRepository<ResXLocalisationDefinitionRepository>();
            var resXEntryRepo = database.GetRepository<ResXLocalisationEntryRepository>();
            var projectFileRepo = database.GetRepository<ProjectFileRepository>();

            var results = new List<ILocalisationDeclaration>();

            var definition = resXDefinitinRepo.GetDefinitionForKey(key);

            if (definition != null)
            {
                var entries = resXEntryRepo.GetEntriesForDefinition(definition);

                foreach (var entry in entries)
                {
                    var file = projectFileRepo.Get(entry.ProjectFileKey);

                    if (file != null)
                    {
                        var projectFile = ProjectService.GetProjectFileWithFilePath(project, file.FilePath);

                        if (projectFile != null)
                        {
                            results.Add(new LocalisationDeclaration(entry, definition.Key, projectFile));
                        }
                    }
                }
            }

            return new LocalisationDeclarationCollection(definition.Key, results);
        }

        public ILocalisationDeclarationCollection ResolveLocalisations(Project project, IPropertySymbol propertySymbol)
        {
            if (propertySymbol is null)
            {
                return null;
            }

            if (project is null)
            {
                return null;
            }

            project = project.Solution.Projects.FirstOrDefault(p => p.AssemblyName == propertySymbol.ContainingAssembly.Name);

            if (project is null)
            {
                return null;
            }

            var outerType = propertySymbol.ContainingType.GetNonAutogeneratedSyntax();

            if (outerType is null)
            {
                return null;
            }

            var fileInfo = new FileInfo(outerType.SyntaxTree.FilePath);

            var fileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);

            if (fileName.EndsWith(".designer", StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - ".designer".Length);
            }

            var resxFilePath = Path.Combine(fileInfo.Directory.FullName, fileName + ".resx");

            var resxProjectFile = ProjectService.GetProjectFileWithFilePath(project, resxFilePath);

            if (resxProjectFile is null)
            {
                return null;
            }

            var key = propertySymbol.Name;

            return ResolveLocalisations(project, key);
        }
    }
}
