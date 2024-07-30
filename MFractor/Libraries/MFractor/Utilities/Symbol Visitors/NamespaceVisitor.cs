using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SymbolVisitors
{
    public class NamespaceSymbolVisitor : SymbolVisitor
    {
        readonly Dictionary<string, INamespaceSymbol> namespaces = new Dictionary<string, INamespaceSymbol>();
        public IReadOnlyList<INamespaceSymbol> NamespaceSymbols => namespaces.Values.ToList();

        public INamespaceSymbol GetNamespaceByName(string @namespace)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                return null;
            }

            namespaces.TryGetValue(@namespace, out var symbol);

            return symbol;
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            foreach (var module in symbol.Modules)
            {
                Visit(module);
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
            if (!symbol.IsGlobalNamespace)
            {
                namespaces[symbol.ToString()] = symbol;
            }
            
            foreach (var ns in symbol.GetNamespaceMembers())
            {
                Visit(ns);
            }
        }
    }
}
