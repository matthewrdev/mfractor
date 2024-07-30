using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.StringsProviders
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILocalisableStringsProviderRepository))]
    class LocalisableStringsProviderRepository : PartRepository<ILocalisableStringsProvider>, ILocalisableStringsProviderRepository
    {
        [ImportingConstructor]
        public LocalisableStringsProviderRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ILocalisableStringsProvider> LocalisableStringsProviders => Parts;

        public ILocalisableStringsProvider GetSupportedStringsProvider(Project project, string filePath)
        {
            return LocalisableStringsProviders.FirstOrDefault(vp => vp.IsAvailable(project, filePath));
        }
    }
}