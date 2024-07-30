using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Indexes;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    /// <summary>
    /// A repository implementatino for <see cref="StyleSetter"/> to provide structure database access.
    /// </summary>
    public class StyleSetterRepository : EntityRepository<StyleSetter>
    {
        public StyleSetterRepository()
            : base(new StyleSetterStyleDefinitionIndex())
        {
        }

        public IReadOnlyList<StyleSetter> GetPropertiesForStyle(StyleDefinition styleDefinition)
        {
            if (styleDefinition == null)
            {
                return new List<StyleSetter>();
            }

            return GetPropertiesForStyle(styleDefinition.PrimaryKey);
        }

        public IReadOnlyList<StyleSetter> GetPropertiesForStyle(int styleDefinitionKey)
        {
            return GetEntityIndex<StyleSetterStyleDefinitionIndex>().GetSettersForStyleDefinitionKey(styleDefinitionKey).Where(setter => !setter.GCMarked).ToList();
        }
    }
}
