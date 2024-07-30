using System.Collections.Generic;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.StringsProviders
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> that provides the <see cref="ILocalisableStringsProvider"/>'s within the app domain.
    /// </summary>
    public interface ILocalisableStringsProviderRepository : IPartRepository<ILocalisableStringsProvider>
    {
        IReadOnlyList<ILocalisableStringsProvider> LocalisableStringsProviders { get; }

        ILocalisableStringsProvider GetSupportedStringsProvider(Project project, string filePath);
    }
}
