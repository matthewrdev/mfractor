using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    public interface IXmlAttributeValue
    {
        string Value { get; set; }
        TextSpan Span { get; set; }
        bool HasValue { get; }
        bool IsClosed { get; set; }

        string ToString();
    }
}