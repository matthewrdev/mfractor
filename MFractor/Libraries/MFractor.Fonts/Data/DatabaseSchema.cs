using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Schemas;
using MFractor.Fonts.Data.Models;

namespace MFractor.Fonts.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DatabaseSchema : Schema
    {
        public override string Domain => "Fonts";

        protected override IReadOnlyDictionary<Type, string> BuildTables()
        {
            return new Dictionary<Type, string>()
                .AddTable<FontFileAsset>()
                .AddTable<FontGlyphClassBinding>();
        }
    }
}
