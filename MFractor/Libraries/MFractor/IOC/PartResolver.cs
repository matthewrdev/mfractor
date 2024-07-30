using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MFractor.IOC
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IPartResolver))]
    class PartResolver : IPartResolver
    {
        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return Resolver.ResolveAll<T>();
        }
    }
}