using System;
using System.Collections.Generic;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SymbolVisitors
{
    public class NamedTypeVisitor : SymbolVisitor
    {
        public Action<INamedTypeSymbol> OnMatchFound { get; set; }

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
            foreach (var type in symbol.GetMembers())
            {
                var namespaceType = type as INamespaceSymbol;
                var typeSymbol = type as INamedTypeSymbol;

                if (typeSymbol != null)
                {
                    OnMatchFound?.Invoke(typeSymbol);
                }
                else if (namespaceType != null)
                {
                    VisitNamespace(namespaceType);
                }
            }
        }
    }
}
