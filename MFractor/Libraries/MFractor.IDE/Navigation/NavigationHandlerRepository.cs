using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using MFractor.Navigation;

namespace MFractor.Navigation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(INavigationHandlerRepository))]
    class NavigationHandlerRepository : PartRepository<INavigationHandler>, INavigationHandlerRepository
    {
        [ImportingConstructor]
        public NavigationHandlerRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<INavigationHandler> NavigationHandlers => Parts;

        public INavigationHandler GetNavigationHandler(Type type)
        {
            if (type is null)
            {
                return default;
            }

            return NavigationHandlers.FirstOrDefault(nh => nh.GetType() == type);
        }
    }
}