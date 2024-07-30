using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Indexes;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    public class ThicknessUsageRepository : EntityRepository<ThicknessUsage>
    {
        public ThicknessUsageRepository()
            : base(new ThicknessUsageFormattedValueIndex())
        {
        }

        public IReadOnlyList<ThicknessUsage> GetThicknessUsagesWithValue(string formattedValue)
        {
            if (string.IsNullOrEmpty(formattedValue))
            {
                return new List<ThicknessUsage>();
            }

            return GetEntityIndex<ThicknessUsageFormattedValueIndex>().GetForFormattedValue(formattedValue).Where(tu => !tu.GCMarked).ToList();
        }

        public int GetCountOfThicknessUsagesWithValue(string formattedValue)
        {
            if (string.IsNullOrEmpty(formattedValue))
            {
                return 0;
            }

            return GetEntityIndex<ThicknessUsageFormattedValueIndex>().GetForFormattedValue(formattedValue).Count(tu => !tu.GCMarked);
        }
    }
}