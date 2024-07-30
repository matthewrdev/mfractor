namespace MFractor.Maui.Syntax
{
    public class TypeNameSyntax : NameSyntax
    {
        public TypeNameSyntax()
            : base(XamlExpressionSyntaxKind.TypeName)
        {
        }

        public string Name { get; internal set; }

        public override string ToString()
        {
            return Leading + Name + Trailing;
        }
    }
}
