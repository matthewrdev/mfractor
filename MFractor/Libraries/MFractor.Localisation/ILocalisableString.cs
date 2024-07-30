using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation
{
    public interface ILocalisableString
    {
        string Value { get; }
        string FilePath { get; }
        TextSpan Span { get; }
        object Syntax { get; }
        bool HasValue { get; }

        bool IsSame(ILocalisableString other);
    }
}