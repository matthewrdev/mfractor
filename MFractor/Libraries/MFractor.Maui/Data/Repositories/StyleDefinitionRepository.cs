using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Indexes;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    /// <summary>
    /// A repository for <see cref="StyleDefinition"/> entities.
    /// </summary>
    public class StyleDefinitionRepository : EntityRepository<StyleDefinition>
    {
        public StyleDefinitionRepository()
            : base(new StyleDefinitionStaticResourceIndex())
        {
        }

        public StyleDefinition GetStyleDefinitionForStaticResource(StaticResourceDefinition staticResourceDefinition)
        {
            if (staticResourceDefinition == null)
            {
                return null;
            }

            return GetStyleDefinitionForStaticResource(staticResourceDefinition.PrimaryKey);
        }

        public StyleDefinition GetStyleDefinitionForStaticResource(int staticResourceDefinitionKey)
        {
            return GetEntityIndex<StyleDefinitionStaticResourceIndex>().GetStyleDefinitionsForStaticResourceKey(staticResourceDefinitionKey).FirstOrDefault(entity => !entity.GCMarked);
        }
    }
}
