using System;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    public class OnPlatformDeclarationRepository : EntityRepository<OnPlatformDeclaration>
    {
        public OnPlatformDeclaration GetOnPlatformDeclarationForStaticResourceDefinition(StaticResourceDefinition staticResourceDefinition)
        {
            if (staticResourceDefinition == null)
            {
                return default;
            }

            return GetOnPlatformDeclarationForStaticResourceDefinition(staticResourceDefinition.PrimaryKey);
        }

        public OnPlatformDeclaration GetOnPlatformDeclarationForStaticResourceDefinition(int staticResourceDefinitionKey)
        {
            return Query(data => data.Values.FirstOrDefault(entity => entity.StaticResourceKey == staticResourceDefinitionKey && !entity.GCMarked));
        }
    }
}
