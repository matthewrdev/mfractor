using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Data.Schemas;
using MFractor.Data;
using MFractor.Images.Data.Models;

namespace MFractor.Images.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DatabaseSchema : Schema
    {
        public override string Domain => "ImageAssets";

        protected override IReadOnlyDictionary<Type, string> BuildTables()
        {
            return new Dictionary<Type, string>().AddTable<ImageAssetDefinition>()
                                                 .AddTable<ImageAssetFile>();
        }
    }
}
