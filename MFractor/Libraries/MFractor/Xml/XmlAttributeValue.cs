using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    public class XmlAttributeValue : XmlSyntax, IXmlAttributeValue
    {
        public XmlAttributeValue(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public TextSpan Span { get; set; }

        public bool HasValue => !string.IsNullOrEmpty(Value);

        /// <summary>
        /// Does this XML attribute have a closing quotation mark.
        /// </summary>
        public bool IsClosed { get; set; } = true;

        public override string ToString()
        {
            return Value;
        }
    }
}

