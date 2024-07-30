using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation
{
    public class LocalisableString : ILocalisableString
    {
        public string Value { get; }

        public string FilePath { get; }

        public TextSpan Span { get; }

        public object Syntax { get; }

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public LocalisableString(string value,
                                 string filePath,
                                 TextSpan span,
                                 object syntax = null)
        {
            Value = value;
            FilePath = filePath;
            Span = span;
            Syntax = syntax;
        }

        public bool IsSame(ILocalisableString other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Value == Value
                   && other.FilePath == FilePath
                   && other.Span == Span;
        }
    }
}
