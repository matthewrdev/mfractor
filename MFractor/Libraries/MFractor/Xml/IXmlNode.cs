using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    public interface IXmlNode
    {
        XmlNode Parent { get; set; }
        XmlName Name { get; set; }
        XmlName ClosingTagName { get; set; }
        string Value { get; set; }
        List<XmlAttribute> Attributes { get; set; }
        List<XmlNode> Children { get; set; }
        bool IsSelfClosing { get; set; }
        bool HasClosingTag { get; set; }
        TextSpan OpeningTagSpan { get; set; }
        TextSpan ClosingTagSpan { get; set; }
        bool IsRoot { get; }
        bool IsLeaf { get; }
        bool HasValue { get; }
        bool HasAttributes { get; }
        bool HasChildren { get; }
        bool HasParent { get; }
        bool ClosingTagNameSpanValid { get; }
        TextSpan ClosingTagNameSpan { get; }
        TextSpan ValueSpan { get; }
        TextSpan Span { get; }
        TextSpan NameSpan { get; }

        void AddAttribute(XmlAttribute attribute);
        void AddAttribute(string name, string value);
        void AddChildNode(XmlNode child);
        void AddChildren(IEnumerable<XmlNode> children);
        XmlNode Clone(bool includeChildren = true, bool includeAttributes = true);
        XmlAttribute GetAttribute(string xmlns, string name);
        XmlAttribute GetAttribute(Func<XmlAttribute, bool> searchFunc);
        XmlAttribute GetAttributeByLocalName(string localName);
        XmlAttribute GetAttributeByName(string attributeName);
        IEnumerable<XmlAttribute> GetAttributes(Func<XmlAttribute, bool> searchFunc);
        XmlNode GetChildAt(int index);
        XmlNode GetChildNode(string xmlns, string localName);
        XmlNode GetChildNode(string fullName);
        XmlNode GetChildNode(Func<XmlNode, bool> searchFunc);
        List<XmlNode> GetChildren(Func<XmlNode, bool> searchFunc);
        bool HasAttribute(string attributeName);
        bool HasAttribute(Func<XmlAttribute, bool> searchFunc);
        bool HasChild(XmlNode node);
        bool HasChild(Func<XmlNode, bool> searchFunc);
        bool HasChildNamed(string fullName);
        string ToString();
    }
}