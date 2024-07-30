using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IUserOptions))]
    class IdeUserOptions : JsonUserOptions
    {
        [ImportingConstructor]
        public IdeUserOptions(Lazy<IApplicationPaths> applicationPaths)
            : base(applicationPaths)
        {
        }
    }
}
