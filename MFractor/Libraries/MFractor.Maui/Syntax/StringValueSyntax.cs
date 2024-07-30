namespace MFractor.Maui.Syntax
{
    public class StringValueSyntax : XamlExpressionSyntaxNode
    {
        public StringValueSyntax()
            : base(XamlExpressionSyntaxKind.StringValue)
        {
        }

        public string Value { get; internal set; }

        public override string ToString()
        {
            return Leading + Value + Trailing;
        }
    }
}
