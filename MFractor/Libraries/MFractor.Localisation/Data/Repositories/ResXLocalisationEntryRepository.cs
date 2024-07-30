using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Localisation.Data.Models;

namespace MFractor.Localisation.Data.Repositories
{
    public class ResXLocalisationEntryRepository : EntityRepository<ResXLocalisationEntry>
    {
        public List<ResXLocalisationEntry> GetEntriesForDefinition(ResXLocalisationDefinition definition)
        {
            return GetEntriesForDefinition(definition.PrimaryKey);
        }

        public List<ResXLocalisationEntry> GetEntriesForDefinition(int definitionKey)
        {
            return Query(d => d.Values.Where(d => d.ResXDefinitionKey == definitionKey && !d.GCMarked).ToList());
        }
    }
}
