using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    class XmlnsDefinition : IXmlnsDefinition
    {
        public XmlnsDefinition(string uri, IEnumerable<INamespaceSymbol> namespaces)
        {
            Uri = uri;
            Namespaces = (namespaces ?? Enumerable.Empty<INamespaceSymbol>()).Where(ns => ns != null).ToList();
            Assemblies = Namespaces.Select(ns => ns.ContainingAssembly).Distinct().ToList();
        }

        public string Uri { get; }

        public IReadOnlyList<IAssemblySymbol> Assemblies { get; }

        public IReadOnlyList<INamespaceSymbol> Namespaces { get; }
    }
}