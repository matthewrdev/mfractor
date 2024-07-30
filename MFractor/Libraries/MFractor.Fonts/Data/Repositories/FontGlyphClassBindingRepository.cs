using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Fonts.Data.Models;

namespace MFractor.Fonts.Data.Repositories
{
    public class FontGlyphClassBindingRepository : EntityRepository<FontGlyphClassBinding>
    {
        public List<FontGlyphClassBinding> GetGlyphClassBindingForFont(IFont font)
        {
            if (font is null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            return Query(data =>
            {
                return data.Values.Where(f => f.FontAssetFileName == font.FileName && !f.GCMarked).ToList();
            });
        }
    }
}
