using System.Collections.Generic;
using System.Linq;

namespace MFractor.Maui.Syntax
{
    public class ExpressionSyntax : XamlExpressionSyntaxNode
    {
        public NameSyntax NameSyntax => GetChild<NameSyntax>();

        public IEnumerable<ElementSyntax> Elements => Children.OfType<ElementSyntax>();

        public ExpressionSyntax()
            : base(XamlExpressionSyntaxKind.Expression)
        {
        }

        internal void AddElement(ElementSyntax elementSyntax)
        {
            AddChild(elementSyntax);
        }
    }
}
