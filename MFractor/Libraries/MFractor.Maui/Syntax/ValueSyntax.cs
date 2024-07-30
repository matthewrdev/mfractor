namespace MFractor.Maui.Syntax
{
    /// <summary>
    /// Right hand component of 'MyProperty=Value' aka 'Value'
    /// </summary>
    public class ValueSyntax : XamlExpressionSyntaxNode
    {
        public ValueSyntax()
            : base(XamlExpressionSyntaxKind.Value)
        {
        }

        public string Value { get; internal set; }

        public override string ToString()
        {
            return Leading + Value + Trailing;
        }
    }
}
