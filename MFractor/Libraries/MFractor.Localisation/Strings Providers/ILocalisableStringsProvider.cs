using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Documents;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.StringsProviders
{
    /// <summary>
    /// A localisable strings provider returns all strings in document that can be replaced with a localisation lookup.
    /// </summary>
    [InheritedExport]
    public interface ILocalisableStringsProvider
	{
        /// <summary>
        /// Given the <paramref name="project"/> and  <paramref name="filePath"/>, does this <see cref="ILocalisableStringsProvider"/> support providing localisable string targets?
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool IsAvailable(Project project, string filePath);

        /// <summary>
        /// Retrieves the localisable strings within the target <paramref name="document"/>.
        /// </summary>
        /// <returns>The localisable strings.</returns>
        /// <param name="document">The document who .</param>
        IEnumerable<ILocalisableString> RetrieveLocalisableStrings(IParsedDocument document, object semanticModel);
    }
}
