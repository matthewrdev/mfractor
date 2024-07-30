namespace MFractor.Maui.Syntax
{
    /// Left hand component of 'MyProperty=Value' aka 'MyProperty'
    public class PropertySyntax : ElementSyntax
    {
        public PropertySyntax()
            : base(XamlExpressionSyntaxKind.Property)
        {
        }

        public string PropertyName { get; internal set; }

        public override string ToString()
        {
            return Leading + PropertyName + Trailing;
        }
    }
}
