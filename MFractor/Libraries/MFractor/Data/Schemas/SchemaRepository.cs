using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Data.Schemas
{
    [Export(typeof(ISchemaRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SchemaRepository : PartRepository<ISchema>, ISchemaRepository
    {
        [ImportingConstructor]
        public SchemaRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ISchema> Schemas => Parts;

        public string GetTableName(Type entityType)
        {
            if (tableNameForTypeCache.ContainsKey(entityType))
            {
                return tableNameForTypeCache[entityType];
            }

            foreach (var schema in Schemas)
            {
                if (schema.HasTableFor(entityType))
                {
                    var tableName = schema.GetTableName(entityType);

                    tableNameForTypeCache[entityType] = tableName;

                    return tableName;
                }
            }

            return string.Empty;
        }

        public string GetTableName<TEntity>()
        {
            return GetTableName(typeof(TEntity));
        }

        readonly Dictionary<Type, string> tableNameForTypeCache = new Dictionary<Type, string>();
    }
}