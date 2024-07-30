using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Data.Repositories
{
    [Export(typeof(IRepositoryCollectionRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class RepositoryCollectionRepository : PartRepository<IRepositoryCollection>, IRepositoryCollectionRepository
    {
        [ImportingConstructor]
        public RepositoryCollectionRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IRepositoryCollection> RepositoryCollections => Parts;
    }
}