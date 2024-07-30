using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Localisation;
using MFractor.Localisation.Data.Models;
using MFractor.Localisation.Data.Repositories;
using MFractor.Localisation.Importing;
using MFractor.Logging;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using MFractor.Workspace.Data.Synchronisation;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Data.Synchronisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ResXDocumentSynchroniser : ITextResourceSynchroniser
    {
        readonly ILogger log = Logger.Create();

        static readonly string[] supportedExtensions = { ".resx" };

        readonly Lazy<IResXLocalisationImporter> resXLocalisationImporter;
        IResXLocalisationImporter ResXLocalisationImporter => resXLocalisationImporter.Value;

        [ImportingConstructor]
        public ResXDocumentSynchroniser(Lazy<IResXLocalisationImporter> resXLocalisationImporter)
        {
            this.resXLocalisationImporter = resXLocalisationImporter;
        }

        public string[] SupportedFileExtensions => supportedExtensions;

        public bool IsAvailable(Solution solution, Project project)
        {
            return true;
        }

        public Task<bool> CanSynchronise(Solution solution, Project project, IProjectFile projectFile)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> Synchronise(Solution solution, Project project, IProjectFile projectFile, ITextProvider textProvider, IProjectResourcesDatabase database)
        {
            IReadOnlyList<ILocalisationValue> localisationValues = null;
            var definitionRepo = database.GetRepository<ResXLocalisationDefinitionRepository>();
            var localisationEntriesRepo = database.GetRepository<ResXLocalisationEntryRepository>();

            try
            {
                var culture = ResXLocalisationImporter.GetCultureInfo(projectFile.FilePath);
                localisationValues = ResXLocalisationImporter.ProvideLocalisationValues(textProvider, culture);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                Debugger.Break();
            }

            var projectFileModel = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(projectFile.FilePath);

            if (localisationValues == null 
                || !localisationValues.Any() 
                || projectFileModel == null)
            {
                return false;
            }

            foreach (var value in localisationValues)
            {
                if (string.IsNullOrEmpty(value.Key) || !value.HasCulture)
                {
                    continue;
                }

                try
                {
                    var definition = definitionRepo.GetOrCreateDefinitionForKey(value.Key);

                    if (definition.GCMarked)
                    {
                        definition.GCMarked = false;
                        definitionRepo.Update(definition);
                    }

                    var entry = new ResXLocalisationEntry();
                    entry.CultureCode = value.Culture.Name;
                    entry.Value = value.Value;
                    entry.SearchValue = value.Value.RemoveDiacritics();
                    entry.ProjectFileKey = projectFileModel.PrimaryKey;
                    entry.ResXDefinitionKey = definition.PrimaryKey;
                    if (value.ValueSpan != null)
                    {
                        entry.ValueStartOffset = value.ValueSpan.Value.Start;
                        entry.ValueEndOffset = value.ValueSpan.Value.End;
                    }

                    if (value.KeySpan != null)
                    {
                        entry.KeyStartOffset = value.KeySpan.Value.Start;
                        entry.KeyEndOffset = value.KeySpan.Value.End;
                    }

                    localisationEntriesRepo.Insert(entry);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            return true;
        }
    }
}
