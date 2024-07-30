using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Configuration;

namespace MFractor.VS.Windows.Services
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

