using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Fonts.Data.Models;

namespace MFractor.Fonts.Data.Repositories
{
    public class FontFileAssetRepository : EntityRepository<FontFileAsset>
    {
        public List<FontFileAsset> GetAllFonts()
        {
            return Query(data =>
            {
                return data.Values.Where(f => !f.GCMarked).ToList();
            });
        }

        public List<FontFileAsset> GetFontsWithPostscriptName(string postscriptName)
        {
            return Query(data =>
            {
                return data.Values.Where(f => f.PostscriptName == postscriptName && !f.GCMarked).ToList();
            });
        }

        public FontFileAsset GetFontFilAssetWithFileName(string fileName)
        {
            return Query(data =>
            {
                return data.Values.FirstOrDefault(f => f.FileName == fileName && !f.GCMarked);
            });
        }
    }
}
