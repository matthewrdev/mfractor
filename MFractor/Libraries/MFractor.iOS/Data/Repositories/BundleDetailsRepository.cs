using System.Linq;
using MFractor.Data.Repositories;
using MFractor.iOS.Data.Models;

namespace MFractor.iOS.Data.Repositories
{
    public class BundleDetailsRepository : EntityRepository<BundleDetails>
    {
        public BundleDetails GetBundleDetails()
        {
            return Query(data => data.OfType<BundleDetails>().FirstOrDefault(bd => !bd.GCMarked));
        }
    }
}
