using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax
{
    public class ExpressionParserState
    {
        public ExpressionParserState Parent { get; }

        public List<ExpressionParserState> Children = new List<ExpressionParserState>();

        public string Content = string.Empty;

        public TextSpan FullSpan { get; set; }

        public TextSpan Span { get; set; }

        public string Leading = string.Empty;

        public string Trailing = string.Empty;

        public bool IsLeaf => !Children.Any();

        public XamlExpressionSyntaxKind Kind;

        public bool IsErrored = false;

        public ExpressionParserState(ExpressionParserState parent, int start, XamlExpressionSyntaxKind syntaxKind)
        {
            Parent = parent;
            FullSpan = TextSpan.FromBounds(start, start);
            Span = TextSpan.FromBounds(start, start);
            Kind = syntaxKind;
        }

        public ExpressionParserState CreateChild(int start, XamlExpressionSyntaxKind syntaxKind)
        {
            var child = new ExpressionParserState(this, start, syntaxKind);
            Children.Add(child);
            return child;
        }

        public void SetFullSpanEnd(int end)
        {
            FullSpan = TextSpan.FromBounds(FullSpan.Start, end);
        }

        public void SetSpanStart(int start)
        {
            Span = TextSpan.FromBounds(start, start);
        }

        public void SetSpanEnd(int end)
        {
            Span = TextSpan.FromBounds(Span.Start, end);
        }

		public override string ToString()
		{
            if (IsLeaf)
            {
                return Leading + Content + Trailing;
            }

            return Leading + string.Join("", Children.Select(c => c.ToString())) + Trailing;
		}
	}

}
