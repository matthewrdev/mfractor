using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Indexes;
using MFractor.Maui.Data.Models;
using MFractor.Utilities;

namespace MFractor.Maui.Data.Repositories
{
    public class ColorDefinitionRepository : EntityRepository<ColorDefinition>
    {
        public ColorDefinitionRepository()
            : base(new ColorDefinitionsByColorIndex())
        {
        }

        public ColorDefinition GetColorForStaticResourceDefinition(StaticResourceDefinition staticResourceDefinition)
        {
            if (staticResourceDefinition == null)
            {
                return default;
            }

            return GetColorForStaticResourceDefinition(staticResourceDefinition.PrimaryKey);
        }

        public ColorDefinition GetColorForStaticResourceDefinition(int staticResourceDefinitionKey)
        {
            return Query(data => data.Values.FirstOrDefault(entity => entity.StaticResourceKey == staticResourceDefinitionKey && !entity.GCMarked));
        }

        public ColorDefinition GetNamedColor(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return default;
            }

            return Query(data => data.Values.FirstOrDefault(entity => entity.Name == name && !entity.GCMarked));
        }

        public IReadOnlyList<ColorDefinition> GetAllDeclaredColors()
        {
            return Query(data => data.Values.Where(entity => !entity.GCMarked).ToList());
        }

        public IReadOnlyList<ColorDefinition> GetColorDefinitionsForColor(Color color)
        {
            return GetColorDefinitionsForColorInteger(color.ToArgb());
        }

        public IReadOnlyList<ColorDefinition> GetColorDefinitionsForColorInteger(int colorInteger)
        {
            return GetEntityIndex<ColorDefinitionsByColorIndex>().GetForColorInteger(colorInteger).Where(c => !c.GCMarked).ToList();
        }

        public bool HasColorDefinitionsForColor(Color color)
        {
            return HasColorDefinitionsForColor(color.ToArgb());
        }

        public bool HasColorDefinitionsForColor(int colorInteger)
        {
            return GetEntityIndex<ColorDefinitionsByColorIndex>().GetForColorInteger(colorInteger).Any(c => !c.GCMarked);
        }

        public IReadOnlyList<ColorDefinition> GetCloselyMatchingColors(Color color)
        {
            return GetEntityIndex<ColorDefinitionsByColorIndex>().GetForColorInteger(color.ToArgb()).Where(c => ColorHelper.ColorsAreClose(c.Color, color) && !c.GCMarked).ToList();
        }
    }
}
