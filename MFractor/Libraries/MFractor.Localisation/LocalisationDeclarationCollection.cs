using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Localisation
{
    class LocalisationDeclarationCollection : ILocalisationDeclarationCollection
    {
        public LocalisationDeclarationCollection(string key, IEnumerable<ILocalisationDeclaration> localisationDeclarations)
        {
            LocalisationDeclarations = (localisationDeclarations ?? Enumerable.Empty<ILocalisationDeclaration>()).ToList();
            Key = key ?? throw new System.ArgumentNullException(nameof(key));
        }

        public IReadOnlyList<ILocalisationDeclaration> LocalisationDeclarations { get; }
        public string Key { get; }

        public IEnumerator<ILocalisationDeclaration> GetEnumerator()
        {
            return LocalisationDeclarations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LocalisationDeclarations.GetEnumerator();
        }
    }
}