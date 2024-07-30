using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IApplicationLifecycleHandlerRepository))]
    class ApplicationLifecycleHandlerRepository : PartRepository<IApplicationLifecycleHandler>, IApplicationLifecycleHandlerRepository
    {
        [ImportingConstructor]
        public ApplicationLifecycleHandlerRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IApplicationLifecycleHandler> ApplicationLifecycleHandlers => Parts;
    }
}