
namespace MFractor.Xml
{
    public interface IXmlSyntaxWriter
    {
        string WriteAttribute(XmlAttribute attribute, IXmlFormattingPolicy policy);

        string WriteNode(XmlNode xmlNode, string indent, IXmlFormattingPolicy policy, bool writeChildren, bool writeClosingTags, bool applyIndentToNode);
    }
}