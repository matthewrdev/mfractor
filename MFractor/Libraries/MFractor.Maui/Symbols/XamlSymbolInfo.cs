using MFractor.Maui.Syntax.Expressions;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Symbols
{
    public class XamlSymbolInfo
    {
        public XmlSyntax Syntax;

        public ITypeSymbol BindingContext;

        public XamlSymbolKind SymbolKind;

        public object Symbol;

        public object AdditionalData; // HACK.

        public Expression Expression;

        public TextSpan Span;

        public TSymbol GetSymbol<TSymbol>()
        {
            try
            {
                return (TSymbol)Symbol;
            }
            catch
            {
            }

            return default;
        }

        public bool IsSymbolType<TSymbol>()
        {
            try
            {
                var temp = (TSymbol)Symbol;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public TSyntax GetSyntax<TSyntax>() where TSyntax : XmlSyntax
        {
            return Syntax as TSyntax;
        }

        public bool IsSyntaxType<TSyntax>() where TSyntax : XmlSyntax
        {
            return Syntax is TSyntax;
        }
    }
}
