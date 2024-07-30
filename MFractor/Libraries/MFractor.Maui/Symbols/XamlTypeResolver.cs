using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Symbols
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlTypeResolver))]
    class XamlTypeResolver : IXamlTypeResolver
    {
        [ImportingConstructor]
        public XamlTypeResolver(Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver)
        {
            this.xmlnsNamespaceSymbolResolver = xmlnsNamespaceSymbolResolver;
        }

        readonly Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver;
        public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver => xmlnsNamespaceSymbolResolver.Value;

        public INamedTypeSymbol ResolveType(string name, IXamlNamespace xamlNamespace, Project project, IXamlPlatform platform)
        {
            if (string.IsNullOrEmpty(name)
                || xamlNamespace is null
                || project is null)
            {
                return null;
            }

            var namespaces = XmlnsNamespaceSymbolResolver.GetNamespaces(xamlNamespace, project, platform);

            return ResolveType(name, project, namespaces);
        }

        public INamedTypeSymbol ResolveType(string name, IXamlNamespace xamlNamespace, Project project, IXmlnsDefinitionCollection xmlnsDefinitions)
        {
            if (string.IsNullOrEmpty(name)
                || xamlNamespace is null
                || xmlnsDefinitions is null)
            {
                return null;
            }

            var namespaces = XmlnsNamespaceSymbolResolver.GetNamespaces(xamlNamespace, project, xmlnsDefinitions);

            return ResolveType(name, project, namespaces);
        }

        public INamedTypeSymbol ResolveType(string name, Project project, IEnumerable<INamespaceSymbol> namespaces)
        {
            if (string.IsNullOrEmpty(name)
                || project is null
                || namespaces is null
                || !namespaces.Any())
            {
                return null;
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            return ResolveType(name, compilation, namespaces);
        }

        public INamedTypeSymbol ResolveType(string name, Compilation compilation, IEnumerable<INamespaceSymbol> namespaces)
        {
            if (string.IsNullOrEmpty(name)
                || compilation is null
                || namespaces is null
                || !namespaces.Any())
            {
                return null;
            }

            foreach (var ns in namespaces)
            {
                var metaTypeName = ns.ToString() + "." + name;

                var typeSymbol = compilation.GetTypeByMetadataName(metaTypeName);

                if (typeSymbol != null)
                {
                    return typeSymbol;
                }
            }

            return null;
        }
    }
}