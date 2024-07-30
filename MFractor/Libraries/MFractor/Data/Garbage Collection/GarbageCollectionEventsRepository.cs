using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Data.GarbageCollection
{
    [Export(typeof(IGarbageCollectionEventsRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GarbageCollectionEventsRepository : PartRepository<IGarbageCollectionEvents>, IGarbageCollectionEventsRepository
    {
        [ImportingConstructor]
        public GarbageCollectionEventsRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IGarbageCollectionEvents> GarbageCollectionEvents => Parts;
    }
}