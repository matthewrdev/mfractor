using System;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SymbolVisitors
{
    public class NamedTypeChildSymbolVisitor : SymbolVisitor
    {
        public Action<INamedTypeSymbol> OnMatchFound { get; set; }

        public string NamedType { get; set;  }

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
                         && SymbolHelper.DerivesFrom(typeSymbol, NamedType))
                {
                    OnMatchFound?.Invoke(typeSymbol);
                }
            }
        }
    }
}
