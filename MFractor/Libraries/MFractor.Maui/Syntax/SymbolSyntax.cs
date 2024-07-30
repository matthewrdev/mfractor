namespace MFractor.Maui.Syntax
{
    public class SymbolSyntax : NameSyntax
    {
        public SymbolSyntax()
            : base(XamlExpressionSyntaxKind.Symbol)
        {
        }

        public NamespaceSyntax NamespaceSyntax => GetChild<NamespaceSyntax>();

        public TypeNameSyntax TypeNameSyntax => GetChild<TypeNameSyntax>();

    }
}
