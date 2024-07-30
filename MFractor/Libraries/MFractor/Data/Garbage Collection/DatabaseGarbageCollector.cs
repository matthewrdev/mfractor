using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using MFractor.Data.Models;
using MFractor.Data.Schemas;
using MFractor.Utilities;

namespace MFractor.Data.GarbageCollection
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDatabaseGarbageCollector))]
    class DatabaseGarbageCollector : IDatabaseGarbageCollector
	{
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IReadOnlyList<Type>> garbageCollectionEntities;
        IReadOnlyList<Type> GarbageCollectionEntities => garbageCollectionEntities.Value;

        readonly Lazy<ISchemaRepository> schemaRepository;
        public ISchemaRepository SchemaRepository => schemaRepository.Value;

        [ImportingConstructor]
        public DatabaseGarbageCollector(Lazy<ISchemaRepository> schemaRepository)
        {
            this.schemaRepository = schemaRepository;

            garbageCollectionEntities = new Lazy<IReadOnlyList<Type>>(() =>
            {
                var types = SchemaRepository.Schemas.SelectMany(s => s.Entities).Distinct();

                var gcEntityType = typeof(GCEntity);

                var gcEntityTypes = types.Where(gcEntityType.IsAssignableFrom);

                return gcEntityTypes.ToList();
            });
        }

        public void GarbageCollect (IDatabase database)
		{
            try
            {
                foreach (var table in GarbageCollectionEntities)
                {
                    var repo = database.GetRepository(table);
                    if (repo != null)
                    {
                        var keys = repo.GetAllEntities().OfType<GCEntity>().Where(gc => gc.GCMarked).Select(gc => gc.PrimaryKey).ToList();
                        repo.DeleteRange(keys);
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}

