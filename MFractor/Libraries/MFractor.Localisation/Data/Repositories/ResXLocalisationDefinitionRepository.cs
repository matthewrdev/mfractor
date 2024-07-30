using System;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Localisation.Data.Models;
using MFractor.Utilities;

namespace MFractor.Localisation.Data.Repositories
{
    public class ResXLocalisationDefinitionRepository : EntityRepository<ResXLocalisationDefinition>
    {
        public ResXLocalisationDefinition GetDefinitionForKey(string key)
        {
            return Query(data =>
            {
                return data.Values.FirstOrDefault(d => d.Key == key);
            });
        }

        public ResXLocalisationDefinition GetOrCreateDefinitionForKey(string key)
        {
            var definition = GetDefinitionForKey(key);

            if (definition == null)
            {
                definition = new ResXLocalisationDefinition
                {
                    PrimaryKey = NextPrimaryKey(),
                    Key = key,
                    SearchName = key.RemoveDiacritics()
                };
                Insert(definition);
            }

            return definition;
        }

        public void UpdateDefinitionHasChildren(ResXLocalisationDefinition definition)
        {
            UpdateDefinitionForKeyHasChildren(definition.Key);
        }

        public void UpdateDefinitionForKeyHasChildren(string key)
        {
            // TODO: This needs to use an index.
            Mutate(data =>
            {
                foreach (var definition in data.Values.Where(d => d.Key == key))
                {
                    definition.GCMarked = false;
                }
            });
        }
    }
}
