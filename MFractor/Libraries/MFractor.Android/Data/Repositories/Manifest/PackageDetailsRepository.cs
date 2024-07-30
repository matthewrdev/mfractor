using System;
using System.Linq;
using MFractor.Android.Data.Models.Manifest;
using MFractor.Data.Repositories;

namespace MFractor.Android.Data.Repositories.Manifest
{
    public class PackageDetailsRepository : EntityRepository<PackageDetails>
    {
        public PackageDetails GetPackageDetails()
        {
            return Query(d => d.Values.FirstOrDefault(d => !d.GCMarked));
        }
    }
}