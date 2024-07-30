using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    public interface IXmlnsDefinition
    {
        public string Uri { get; }

        public IReadOnlyList<INamespaceSymbol> Namespaces { get; }

        public IReadOnlyList<IAssemblySymbol> Assemblies { get; }
    }
}