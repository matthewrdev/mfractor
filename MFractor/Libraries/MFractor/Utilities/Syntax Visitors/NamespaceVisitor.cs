using System;
using System.Threading;

using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SyntaxVisitors
{
	public class NamespaceVisitor : SymbolVisitor
	{
		public readonly string SearchValue;

		public readonly CancellationToken Token;

		public NamespaceVisitor()
		{
		}

		public INamespaceSymbol MatchedSymbol
		{
			get;
			private set;
		}

		public bool ContinueSearch
		{
			get;
			set;
		} = true;

		public NamespaceVisitor(string searchName, CancellationToken token = default(CancellationToken))
		{
			SearchValue = searchName;
			Token = token;
		}

		public override void VisitAssembly(IAssemblySymbol symbol)
		{
			Token.ThrowIfCancellationRequested();
			symbol.Accept(this);
		}

		public override void VisitModule(IModuleSymbol symbol)
		{
			Token.ThrowIfCancellationRequested();
			symbol.GlobalNamespace.Accept(this);
		}

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            Token.ThrowIfCancellationRequested();
            if (!ContinueSearch)
            {
                return;
            }

            if (symbol.ToString() == SearchValue)
            {
                SubmitResult(symbol);
                return;
            }

            foreach (var m in symbol.GetNamespaceMembers())
            {
                m.Accept(this);
            }
        }

		void SubmitResult(INamespaceSymbol result)
		{
			ContinueSearch = false;
			this.MatchedSymbol = result;
		}
	}
}

