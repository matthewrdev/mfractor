using System;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.GarbageCollection;

namespace MFractor.Localisation.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GarbageCollectionEvents : IGarbageCollectionEvents
    {
        public void OnGarbageCollectionStarted(IDatabase database)
        {
            // TODO: Find all childless resx definitions.
        }

        public void OnGarbageCollectionEnded(IDatabase database)
        {
        }

    }
}
