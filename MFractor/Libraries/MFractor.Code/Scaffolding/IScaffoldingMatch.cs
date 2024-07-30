using Microsoft.CodeAnalysis.Text;

namespace MFractor.Code.Scaffolding
{
    public interface IScaffoldingMatch
    {
        TextSpan Span { get; }

        string Description { get; }
    }
}
