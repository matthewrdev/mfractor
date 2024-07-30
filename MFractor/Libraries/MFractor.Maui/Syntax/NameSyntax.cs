namespace MFractor.Maui.Syntax
{
    public abstract class NameSyntax : XamlExpressionSyntaxNode
    {
        public NameSyntax(XamlExpressionSyntaxKind syntaxKind)
            : base(syntaxKind)
        {
        }
    }
}
