using System.Collections.Generic;
using System.Linq;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Linker.CodeGeneration
{
    class LinkerFileData
    {
        public List<IAssemblySymbol> Assemblies { get; } = new List<IAssemblySymbol>();

        public Dictionary<IAssemblySymbol, List<INamespaceSymbol>> Namespaces { get; } = new Dictionary<IAssemblySymbol, List<INamespaceSymbol>>();

        public Dictionary<IAssemblySymbol, List<INamedTypeSymbol>> NamedTypes { get; } = new Dictionary<IAssemblySymbol, List<INamedTypeSymbol>>();

        public Dictionary<INamedTypeSymbol, List<ISymbol>> Members { get; } = new Dictionary<INamedTypeSymbol, List<ISymbol>>();

        public LinkerFileData(IEnumerable<ISymbol> symbols)
        {
            foreach (var symbol in (symbols ?? new List<ISymbol>()))
            {
                Add(symbol);
            }
        }

        public void Add(ISymbol symbol)
        {
            if (symbol is IAssemblySymbol assembly)
            {
                Assemblies.Add(assembly);
            }
            else if (symbol is INamedTypeSymbol namedType)
            {
                if (!NamedTypes.ContainsKey(@namedType.ContainingAssembly))
                {
                    NamedTypes[@namedType.ContainingAssembly] = new List<INamedTypeSymbol>();
                }
                NamedTypes[@namedType.ContainingAssembly].Add(namedType);
                Add(namedType.ContainingAssembly);
            }
            else if (symbol is INamespaceSymbol @namespace)
            {
                if (!Namespaces.ContainsKey(@namespace.ContainingAssembly))
                {
                    Namespaces[@namespace.ContainingAssembly] = new List<INamespaceSymbol>();
                }
                Namespaces[@namespace.ContainingAssembly].Add(@namespace);
                Add(@namespace.ContainingAssembly);
            }
            else if (symbol is IFieldSymbol
                     || symbol is IMethodSymbol
                     || symbol is IPropertySymbol
                     || symbol is IEventSymbol)
            {
                var type = SymbolHelper.GetMemberContainingType(symbol);

                if (!Members.ContainsKey(@type))
                {
                    Members[type] = new List<ISymbol>();
                }
                Members[type].Add(symbol);
                Add(type);
            }
        }

        public void Deduplicate()
        {
            var duplicateAssemblies = Assemblies.GroupBy(x => x)
             .Where(g => g.Count() > 1)
             .Select(y => y.Key)
             .ToList();

            if (duplicateAssemblies.Any())
            {
                foreach (var da in duplicateAssemblies)
                {
                    Assemblies.Remove(da);
                }
            }

            foreach (var values in Namespaces.Values)
            {
                var dupes = values.GroupBy(x => x)
                         .Where(g => g.Count() > 1)
                         .Select(y => y.Key)
                         .ToList();

                foreach (var d in dupes)
                {
                    values.Remove(d);
                }
            }

            foreach (var values in NamedTypes.Values)
            {
                var dupes = values.GroupBy(x => x)
                         .Where(g => g.Count() > 1)
                         .Select(y => y.Key)
                         .ToList();

                foreach (var d in dupes)
                {
                    values.Remove(d);
                }
            }


            foreach (var values in Members.Values)
            {
                var dupes = values.GroupBy(x => x)
                         .Where(g => g.Count() > 1)
                         .Select(y => y.Key)
                         .ToList();

                foreach (var d in dupes)
                {
                    values.Remove(d);
                }
            }
        }
    }
}