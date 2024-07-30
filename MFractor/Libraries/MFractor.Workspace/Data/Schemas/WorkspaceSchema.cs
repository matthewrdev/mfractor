using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Schemas;
using MFractor.Workspace.Data.Models;

namespace MFractor.Workspace.Data.Schemas
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class WorkspaceSchema : Schema
    {
        public override string Domain => string.Empty;

        protected override IReadOnlyDictionary<Type, string> BuildTables()
        {
            return new Dictionary<Type, string>().AddTable<ProjectFile>();
        }
    }
}
