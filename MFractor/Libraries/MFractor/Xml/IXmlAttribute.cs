using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    public interface IXmlAttribute
    {
        XmlNode Parent { get; set; }
        XmlName Name { get; set; }
        XmlAttributeValue Value { get; set; }
        TextSpan Span { get; set; }
        TextSpan NameSpan { get; }
        bool HasValue { get; }

        string ToString();
    }
}