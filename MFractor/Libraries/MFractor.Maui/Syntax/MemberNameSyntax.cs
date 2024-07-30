namespace MFractor.Maui.Syntax
{
    public class MemberNameSyntax : ElementSyntax
    {
        public string MemberName { get; internal set; }

        public MemberNameSyntax()
            : base(XamlExpressionSyntaxKind.MemberName)
        {
        }

        public override string ToString()
        {
            return Leading + MemberName + Trailing;
        }
    }
}
