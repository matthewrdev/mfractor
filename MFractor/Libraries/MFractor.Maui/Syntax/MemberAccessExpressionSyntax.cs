using System.Collections.Immutable;
using System.Linq;

namespace MFractor.Maui.Syntax
{
    public class MemberAccessExpressionSyntax : ElementSyntax
    {
        public MemberAccessExpressionSyntax()
            : base(XamlExpressionSyntaxKind.MemberAccessExpression)
        {
        }

        public NameSyntax NameSyntax => GetChild<NameSyntax>();

        public ImmutableList<MemberNameSyntax> Members => Children.OfType<MemberNameSyntax>().ToImmutableList();

        internal void AddMember(MemberNameSyntax memberNameSyntax)
        {
            AddChild(memberNameSyntax);
        }
    }
}
