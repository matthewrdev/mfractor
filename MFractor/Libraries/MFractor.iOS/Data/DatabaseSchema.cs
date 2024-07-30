using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using MFractor.Data;
using MFractor.Data.Schemas;
using MFractor.iOS.Data.Models;

namespace MFractor.iOS.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DatabaseSchema : Schema
    {
        public override string Domain => "iOS";

        protected override IReadOnlyDictionary<Type, string> BuildTables() => 
            new Dictionary<Type, string>()
                .AddTable<BundleDetails>();
    }
}
