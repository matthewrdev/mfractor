using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation
{
    public interface ILocalisationResolver
    {
        ILocalisationDeclarationCollection ResolveLocalisations(Project project, IPropertySymbol propertySymbol);
        ILocalisationDeclarationCollection ResolveLocalisations(Project project, string key);
    }
}
