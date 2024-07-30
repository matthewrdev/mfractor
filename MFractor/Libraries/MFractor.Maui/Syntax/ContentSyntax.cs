namespace MFractor.Maui.Syntax
{
    public class ContentSyntax : ElementSyntax
    {
        public ContentSyntax()
            : base(XamlExpressionSyntaxKind.Content)
        {
        }

        public string Content { get; internal set; }

        public override string ToString()
        {
            return Leading + Content + Trailing;
        }
    }
}
