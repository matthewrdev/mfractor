using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Indexes;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Utilities;

namespace MFractor.Maui.Data.Repositories
{
    public class ThicknessDefinitionRepository : EntityRepository<ThicknessDefinition>
    {
        public ThicknessDefinitionRepository()
            : base(new ThicknessDefinitionFormattedValueIndex())
        {

        }

        public ThicknessDefinition GetThicknessForStaticResourceDefinition(StaticResourceDefinition staticResourceDefinition)
        {
            if (staticResourceDefinition == null)
            {
                return default;
            }

            return GetThicknessForStaticResourceDefinition(staticResourceDefinition.PrimaryKey);
        }

        public ThicknessDefinition GetThicknessForStaticResourceDefinition(int StaticResourceKey)
        {
            return Query(data => data.Values.FirstOrDefault(entity => entity.StaticResourceKey == StaticResourceKey && !entity.GCMarked));
        }

        public IReadOnlyList<ThicknessDefinition> FindMatchingThicknessDefinitions(double left, double right, double top, double bottom)
        {
            var formattedValue = ThicknessHelper.ToFormattedValueString(left, right, top, bottom);

            return FindMatchingThicknessDefinitions(formattedValue);
        }

        public IReadOnlyList<ThicknessDefinition> FindMatchingThicknessDefinitions(string formattedValueString)
        {
            if (string.IsNullOrEmpty(formattedValueString))
            {
                throw new ArgumentException($"'{nameof(formattedValueString)}' cannot be null or empty.", nameof(formattedValueString));
            }

            return GetEntityIndex<ThicknessDefinitionFormattedValueIndex>().GetForFormattedValue(formattedValueString).Where(td => !td.GCMarked).ToList();
        }
    }
}
