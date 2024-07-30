using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Schemas;
using MFractor.Localisation.Data.Models;

namespace MFractor.Localisation.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DatabaseSchema : Schema
    {
        public override string Domain => "Localisation";

        protected override IReadOnlyDictionary<Type, string> BuildTables()
        {
            return new Dictionary<Type, string>().AddTable<ResXLocalisationDefinition>()
                                                 .AddTable<ResXLocalisationEntry>();
        }
    }
}
