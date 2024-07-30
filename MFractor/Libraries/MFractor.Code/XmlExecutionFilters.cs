using MFractor.Code.Documents;
using MFractor.Xml;

namespace MFractor.Code
{
    public static class XmlExecutionFilters
    {
        public static readonly DocumentExecutionFilter XmlDocument = new DocumentExecutionFilter("XML Document", document => document is ParsedXmlDocument, null);

        public static readonly DocumentExecutionFilter XmlNode = new DocumentExecutionFilter("XML Node", document => document is ParsedXmlDocument, (syntax) => syntax is IXmlNode);

        public static readonly DocumentExecutionFilter XmlAttribute = new DocumentExecutionFilter("XML Attribute", document => document is ParsedXmlDocument, (syntax) => syntax is IXmlAttribute);

        public static readonly DocumentExecutionFilter XmlAttributeValue = new DocumentExecutionFilter("XML Attribute", document => document is ParsedXmlDocument, (syntax) => syntax is IXmlAttributeValue);
    }
}
