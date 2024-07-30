using System.Drawing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.CSharp.Services
{
    public class ColorEvalautionResult
    {
        public TextSpan Span { get; }

        public SyntaxNode Syntax { get; }

        public Color Color { get; }
    }
}