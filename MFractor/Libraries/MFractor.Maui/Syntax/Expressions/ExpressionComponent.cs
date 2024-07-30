using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    public class ExpressionComponent
    {
        public readonly string Content;
        public readonly TextSpan Span;

        public override string ToString()
        {
            return Content;
        }

        public ExpressionComponent(string content, TextSpan span)
        {
            Content = content;
            Span = span;
        }

        public ExpressionComponent(string content, int start, int end)
            : this(content, TextSpan.FromBounds(start, end))
        {
        }

        public bool HasContent => !string.IsNullOrEmpty(Content);
    }
}
