using System.Globalization;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation
{
    public interface ILocalisationValue
    {
        string Key { get; }

        TextSpan? KeySpan { get; }

        string Value { get; }

        TextSpan? ValueSpan { get; }

        string Comment { get; }

        TextSpan? CommentSpan { get; }

        CultureInfo Culture { get; }

        bool HasCulture { get; }
    }
}