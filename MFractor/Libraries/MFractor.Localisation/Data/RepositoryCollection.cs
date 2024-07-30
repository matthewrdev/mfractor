using System;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Localisation.Data.Models;
using MFractor.Localisation.Data.Repositories;

namespace MFractor.Localisation.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class RepositoryCollection : IRepositoryCollection
    {
        public void RegisterRepositories(IDatabase database)
        {
            database.RegisterRepository<ResXLocalisationDefinitionRepository, ResXLocalisationDefinition>(new ResXLocalisationDefinitionRepository());
            database.RegisterRepository<ResXLocalisationEntryRepository, ResXLocalisationEntry>(new ResXLocalisationEntryRepository());
        }
    }
}
