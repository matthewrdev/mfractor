using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Android.Data.Models;
using MFractor.Android.Data.Models.Manifest;
using MFractor.Data;
using MFractor.Data.Schemas;

namespace MFractor.Fonts.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DatabaseSchema : Schema
    {
        public override string Domain => "Android";

        protected override IReadOnlyDictionary<Type, string> BuildTables()
        {
            return new Dictionary<Type, string>()
                .AddTable<IdentifierUsage>()
                .AddTable<IdentifierDefinition>()
                .AddTable<PackageDetails>()
                .AddTable<Permission>();
        }
    }
}
