using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IConfigurableRepository))]
    class ConfigurableRepository : PartRepository<IConfigurable>, IConfigurableRepository
    {
        [ImportingConstructor]
        public ConfigurableRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IConfigurable> Configurables => Parts;
    }
}