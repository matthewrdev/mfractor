using System;
using System.Collections.Generic;
using MFractor.IOC;
using MFractor.Localisation.Importing;

namespace MFractor.Localisation.ValueProviders
{
    public interface ILocalisationValuesProviderRepository : IPartRepository<ILocalisationValuesProvider>
    {
        IReadOnlyList<ILocalisationValuesProvider> LocalisationValuesProviders { get; }

        ILocalisationValuesProvider GetSupportedValuesProvider(Microsoft.CodeAnalysis.Project project);
    }
}
