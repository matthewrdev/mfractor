using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Workspace.Data.Synchronisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAssetResourceSynchroniserRepository))]
    class AssetResourceSynchroniserRepository : PartRepository<IAssetResourceSynchroniser>, IAssetResourceSynchroniserRepository
    {
        [ImportingConstructor]
        public AssetResourceSynchroniserRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IAssetResourceSynchroniser> Synchronisers => Parts;
    }
}