using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Indexes;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    public class ColorUsageRepository : EntityRepository<ColorUsage>
    {
        public ColorUsageRepository()
            : base(new ColorUsageColorIntegerIndex())
        {
        }

        public IReadOnlyList<ColorUsage> GetColorUsagesWithValue(Color color)
        {
            return GetColorUsagesWithValue(color.ToArgb());
        }

        public IReadOnlyList<ColorUsage> GetColorUsagesWithValue(int colorInteger)
        {
            return GetEntityIndex<ColorUsageColorIntegerIndex>().GetForColorInteger(colorInteger).Where(entity => !entity.GCMarked).ToList();
        }

        public IReadOnlyList<ColorUsage> GetHexadecimalColorUsagesWithValue(Color color)
        {
            return GetHexadecimalColorUsagesWithValue(color.ToArgb());
        }

        public IReadOnlyList<ColorUsage> GetHexadecimalColorUsagesWithValue(int colorInteger)
        {
            return GetEntityIndex<ColorUsageColorIntegerIndex>().GetForColorInteger(colorInteger).Where(entity => entity.IsHexColor && !entity.GCMarked).ToList();
        }

        public int GetCountOfColorUsagesWithValue(Color color)
        {
            return GetCountOfColorUsagesWithValue(color.ToArgb());
        }

        public int GetCountOfColorUsagesWithValue(int colorInteger)
        {
            return GetEntityIndex<ColorUsageColorIntegerIndex>().GetForColorInteger(colorInteger).Count(entity => !entity.GCMarked);
        }

        public int GetCountOfHexadecimalColorUsagesWithValue(Color color)
        {
            return GetCountOfHexadecimalColorUsagesWithValue(color.ToArgb());
        }

        public int GetCountOfHexadecimalColorUsagesWithValue(int colorInteger)
        {
            return GetEntityIndex<ColorUsageColorIntegerIndex>().GetForColorInteger(colorInteger).Count(entity => entity.IsHexColor && !entity.GCMarked);
        }
    }
}