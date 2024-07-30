using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using MFractor.Localisation.Importing;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.ValueProviders
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILocalisationValuesProviderRepository))]
    class LocalisationValuesProviderRepository : PartRepository<ILocalisationValuesProvider>, ILocalisationValuesProviderRepository
    {
        [ImportingConstructor]
        public LocalisationValuesProviderRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ILocalisationValuesProvider> LocalisationValuesProviders => Parts;

        public ILocalisationValuesProvider GetSupportedValuesProvider(Project project)
        {
            return Parts.FirstOrDefault(vp => vp.CanProvide(project, out var message));
        }
    }
}