namespace MFractor.Maui.Syntax
{
    // 'MyProperty=Value'
    public class AssignmentSyntax : ElementSyntax
    {
        public AssignmentSyntax()
            : base(XamlExpressionSyntaxKind.Assignment)
        {
        }

        public PropertySyntax PropertySyntax => GetChild<PropertySyntax>();

        public XamlExpressionSyntaxNode ValueSyntax => GetChild<XamlExpressionSyntaxNode>(s => !(s is PropertySyntax));
    }
}
