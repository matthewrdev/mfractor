using System;
using System.Linq;
using MFractor.Maui.Xmlns;
using MFractor.IOC;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Utilities
{
    public static class PrefixHelper
    {
        readonly static Lazy<IXmlnsNamespaceSymbolResolver> xmlnsNamespaceSymbolResolver = new Lazy<IXmlnsNamespaceSymbolResolver>(Resolver.Resolve<IXmlnsNamespaceSymbolResolver>);
        public static IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver => xmlnsNamespaceSymbolResolver.Value;

        public static string GetPrefixForType(INamedTypeSymbol typeSymbol,
                                              Project project,
                                              IXmlnsDefinitionCollection xmlnsDefinitions,
                                              IXamlNamespaceCollection xamlNamespaces)
        {
            IXamlNamespace xmlns = default;

            foreach (var ns in xamlNamespaces)
            {
                var namespaces = XmlnsNamespaceSymbolResolver.GetNamespaces(ns, project, xmlnsDefinitions);

                if (namespaces.Any(n => n.ToString() == typeSymbol.ContainingNamespace.ToString()))
                {
                    xmlns = ns;
                    break;
                }
            }

            var prefix = "";
            if (xmlns != null)
            {
                prefix = !string.IsNullOrEmpty(xmlns.Prefix) ? xmlns.Prefix + ":" : "";
            }

            return prefix;
        }
    }
}
