using System.Collections.Generic;
using MFractor.Maui.Syntax.Expressions;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.DataBinding
{
    class BindingAnalysisBundle
    {
        public BindingExpression Expression;

        public ISymbol SuggestedSymbol;

        public List<ISymbol> SymbolPath;

        public string SuggestedPath;

        public string Prefix;
    }
}
