using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace MFractor.Utilities.SymbolVisitors
{
    public class FindNamedTypeVisitor : SymbolVisitor
    {
        public readonly string SearchTypeName;

        public CancellationToken Token = CancellationToken.None;

        public bool FuzzySearch { get; set; } = false;

        public int FuzzySearchDistance { get; set; } = 3;

        public INamedTypeSymbol MatchedSymbol
        {
            get;
            private set;
        }

        public bool ContinueSearch
        {
            get;
            set;
        } = true;

        public FindNamedTypeVisitor(string searchName, CancellationToken token = default)
        {
            SearchTypeName = searchName;
            Token = token;
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            Token.ThrowIfCancellationRequested();
            foreach (var m in symbol.Modules)
            {
                if (!ContinueSearch)
                {
                    break;
                }

                m.Accept(this);
            }
        }

        public override void VisitModule(IModuleSymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            Token.ThrowIfCancellationRequested();

            foreach (var type in symbol.GetMembers())
            {
                if (!ContinueSearch)
                {
                    break;
                }

                type.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            Token.ThrowIfCancellationRequested();

            if (FuzzySearch)
            {
                var distance = LevenshteinDistanceHelper.Compute(symbol.Name, SearchTypeName);
                if (distance < FuzzySearchDistance)
                {
                    SubmitResult(symbol);
                }
            }
            else
            {
                if (symbol.Name == SearchTypeName)
                {
                    SubmitResult(symbol);
                }
            }
        }

        void SubmitResult(INamedTypeSymbol result)
        {
            ContinueSearch = false;
            this.MatchedSymbol = result;
        }
    }
}

