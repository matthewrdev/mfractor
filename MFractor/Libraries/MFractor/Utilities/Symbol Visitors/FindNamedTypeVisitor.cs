using System;
using System.Collections.Generic;
using System.Threading;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SymbolVisitors
{
    public class FindNamedTypeImplementationsVisitor : SymbolVisitor
	{
        public string TypeName { get; }

		public CancellationToken Token = default(CancellationToken);

        public List<INamedTypeSymbol> Matches
        {
            get;
        } = new List<INamedTypeSymbol>();

        public FindNamedTypeImplementationsVisitor(string typeName, CancellationToken token = default(CancellationToken))
		{
			TypeName = typeName;
			Token = token;
		}

		public override void VisitAssembly(IAssemblySymbol symbol)
		{
            if (Token.IsCancellationRequested)
            {
                return;
            }

			foreach (var m in symbol.Modules)
			{
				m.Accept(this);
			}
		}

		public override void VisitModule(IModuleSymbol symbol)
        {
            if (Token.IsCancellationRequested)
            {
                return;
            }

			symbol.GlobalNamespace.Accept(this);
		}

		public override void VisitNamespace(INamespaceSymbol symbol)
        {
            if (Token.IsCancellationRequested)
            {
                return;
            }

			foreach (var type in symbol.GetMembers())
            {
                if (Token.IsCancellationRequested)
                {
                    return;
                }

				type.Accept(this);
			}
		}

		public override void VisitNamedType(INamedTypeSymbol symbol)
		{
			Token.ThrowIfCancellationRequested();

            if (symbol != null
                && !symbol.IsAbstract
                && SymbolHelper.DerivesFrom(symbol, TypeName))
            {
                Matches.Add(symbol);
            }
		}
	}
}

