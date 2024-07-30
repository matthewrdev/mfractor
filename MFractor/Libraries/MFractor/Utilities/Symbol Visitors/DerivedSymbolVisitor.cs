using System;
using System.Collections.Generic;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SymbolVisitors
{
    public class DerivedSymbolVisitor : SymbolVisitor
    {
        readonly List<INamedTypeSymbol> symbols = new List<INamedTypeSymbol>();
        public IReadOnlyList<INamedTypeSymbol> Symbols => symbols;

        public DerivedSymbolVisitor(string baseType)
        {
            BaseType = baseType;
        }

        public string BaseType { get; }

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
            foreach (var type in symbol.GetMembers())
            {
                var namespaceType = type as INamespaceSymbol;
                var typeSymbol = type as INamedTypeSymbol;

                if (namespaceType != null)
                {
                    type.Accept(this);
                }
                else if (typeSymbol != null
                         && !typeSymbol.IsAbstract
                         && SymbolHelper.DerivesFrom(typeSymbol, BaseType))
                {
                    symbols.Add(typeSymbol);
                }
            }
        }
    }
}
