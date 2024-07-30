using System.Collections.Generic;

namespace MFractor.Localisation
{
    public interface ILocalisationDeclarationCollection : IEnumerable<ILocalisationDeclaration>
    {
        string Key { get; }

        IReadOnlyList<ILocalisationDeclaration> LocalisationDeclarations { get; }
    }
}
