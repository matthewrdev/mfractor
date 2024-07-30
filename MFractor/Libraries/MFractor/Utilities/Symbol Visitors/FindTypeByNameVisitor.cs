using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SymbolVisitors
{
    public class FindTypeByNameVisitor : SymbolVisitor
    {
        public string TypeName { get; }
        public List<INamespaceSymbol> ExcludedNamespaces { get; }
        public List<INamedTypeSymbol> Matches { get; } = new List<INamedTypeSymbol>();

        public FindTypeByNameVisitor(string typeName, List<INamespaceSymbol> excludedNamespaces = null)
        {
            TypeName = typeName;
            ExcludedNamespaces = excludedNamespaces ?? new List<INamespaceSymbol>();
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            if (symbol.TypeNames.Contains(TypeName))
            {
                foreach (var module in symbol.Modules)
                {
                    Visit(module);
                }
            }
        }

        public override void VisitModule(IModuleSymbol symbol)
        {
            if (symbol == null)
            {
                return;
            }

            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var ns in symbol.GetNamespaceMembers())
            {
                Visit(ns);
            }

            if (!ExcludedNamespaces.Contains(symbol))
            {
                var types = symbol.GetTypeMembers().Where(t => t.Name.Equals(TypeName, StringComparison.InvariantCulture));

                if (types.Any())
                {
                    Matches.AddRange(types);
                }
            }
        }
    }
}
