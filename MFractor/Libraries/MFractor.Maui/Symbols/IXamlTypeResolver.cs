using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Symbols
{
    public interface IXamlTypeResolver
    {
        INamedTypeSymbol ResolveType(string name, IXamlNamespace xamlNamespace, Project project, IXamlPlatform platform);
        INamedTypeSymbol ResolveType(string name, IXamlNamespace xamlNamespace, Project project, IXmlnsDefinitionCollection xmlnsDefinitions);
        INamedTypeSymbol ResolveType(string name, Project project, IEnumerable<INamespaceSymbol> namespaces);
        INamedTypeSymbol ResolveType(string name, Compilation compilation, IEnumerable<INamespaceSymbol> namespaces);
    }
}