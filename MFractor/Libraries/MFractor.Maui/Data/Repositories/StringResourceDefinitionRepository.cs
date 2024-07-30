using System;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    public class StringResourceDefinitionRepository : EntityRepository<StringResourceDefinition>
    {
        public StringResourceDefinition GetStringForStaticResourceDefinition(StaticResourceDefinition staticResourceDefinition)
        {
            if (staticResourceDefinition == null)
            {
                return default;
            }

            return GetStringForStaticResourceDefinition(staticResourceDefinition.PrimaryKey);
        }

        public StringResourceDefinition GetStringForStaticResourceDefinition(int staticResourceDefinitionKey)
        {
            return Query(data => data.Values.FirstOrDefault(entity => entity.StaticResourceKey == staticResourceDefinitionKey && !entity.GCMarked));
        }
    }
}