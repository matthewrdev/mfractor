using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.iOS.Data.Models;
using MFractor.iOS.Data.Repositories;

namespace MFractor.iOS.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class RepositoryCollection : IRepositoryCollection
    {
        public void RegisterRepositories(IDatabase database)
        {
            database.RegisterRepository<BundleDetailsRepository, BundleDetails>(new BundleDetailsRepository());
        }
    }
}
