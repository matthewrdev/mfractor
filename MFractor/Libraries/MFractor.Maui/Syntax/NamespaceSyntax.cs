namespace MFractor.Maui.Syntax
{
    public class NamespaceSyntax : NameSyntax
    {
        public NamespaceSyntax()
            : base(XamlExpressionSyntaxKind.Namespace)
        {
        }

        public string Namespace { get; internal set; }

        public override string ToString()
        {
            string content = Leading;

            if (!string.IsNullOrEmpty(Namespace))
            {
                content += Namespace;
            }

            content += Trailing;

            return content;
        }
    }
}
