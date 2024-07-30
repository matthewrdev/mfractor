namespace MFractor.Maui.Syntax
{
    public abstract class ElementSyntax : XamlExpressionSyntaxNode
    {
        public ElementSyntax(XamlExpressionSyntaxKind syntaxKind)
            : base(syntaxKind)
        {
        }
    }
}
