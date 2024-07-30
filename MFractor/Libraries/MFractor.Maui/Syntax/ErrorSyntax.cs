namespace MFractor.Maui.Syntax
{
    public class ErrorSyntax : XamlExpressionSyntaxNode
    {
        public string Content { get; internal set; }

        public ErrorSyntax()
            : base(XamlExpressionSyntaxKind.Error)
        {
        }

        public override string ToString()
        {
            return Leading + Content + Trailing;
        }
    }
}
