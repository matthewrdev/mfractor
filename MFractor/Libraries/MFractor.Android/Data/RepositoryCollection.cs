using System.ComponentModel.Composition;
using MFractor.Android.Data.Models.Manifest;
using MFractor.Android.Data.Repositories.Manifest;
using MFractor.Data;
using MFractor.Data.Repositories;

namespace MFractor.Android.Data
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class RepositoryCollection : IRepositoryCollection
    {
        public void RegisterRepositories(IDatabase database)
        {
            database.RegisterRepository<PackageDetailsRepository, PackageDetails>(new PackageDetailsRepository());
        }
    }
}
