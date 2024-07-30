using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    public interface IXmlnsNamespaceSymbolResolver
    {
        IEnumerable<INamespaceSymbol> GetNamespaces(IXamlSchema xamlSchema, Project project, IXamlPlatform platform);
        IEnumerable<INamespaceSymbol> GetNamespaces(IXamlSchema xamlSchema, IXmlnsDefinitionCollection xmlnsDefinitions);

        IEnumerable<INamespaceSymbol> GetNamespaces(IXamlNamespace xamlNamespace, Project project, IXamlPlatform platform);
        IEnumerable<INamespaceSymbol> GetNamespaces(IXamlNamespace xamlNamespace, Project project, IXmlnsDefinitionCollection xmlnsDefinitions);

        IEnumerable<IAssemblySymbol> GetAssemblies(IXamlSchema xamlSchema, Project project, IXamlPlatform platform);
        IEnumerable<IAssemblySymbol> GetAssemblies(IXamlSchema xamlSchema, IXmlnsDefinitionCollection xmlnsDefinitions);

        IEnumerable<IAssemblySymbol> GetAssemblies(IXamlNamespace xamlNamespace, Project project, IXamlPlatform platform);
        IEnumerable<IAssemblySymbol> GetAssemblies(IXamlNamespace xamlNamespace, Project project, IXmlnsDefinitionCollection xmlnsDefinitions);
    }
}