using System;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    /// <summary>
    /// Represents an XML attribute within an XML node.
    /// </summary>
    public class XmlAttribute : XmlSyntax, IXmlAttribute
    {
        public XmlAttribute()
        {
        }

        public XmlAttribute(string name, string value)
        {
            Name = new XmlName(name);
            Value = new XmlAttributeValue(value);
        }

        /// <summary>
        /// The <see cref="XmlNode"/> that is the parent of this attribute.
        /// </summary>
		public XmlNode Parent { get; set; }

        /// <summary>
        /// The <see cref="XmlName"/> component of this attribute.
        /// </summary>
		public XmlName Name { get; set; }

        /// <summary>
        /// Gets the value of this attribute.
        /// <para/>
        /// May be null, use <see cref="HasValue"/> to verify that this attribute does indeed have a value.
        /// </summary>
        public XmlAttributeValue Value { get; set; }

        /// <summary>
        /// The full <see cref="TextSpan"/> of this attribute.
        /// </summary>
        public TextSpan Span { get; set; }

        /// <summary>
        /// The <see cref="TextSpan"/> of the name component of this attribute.
        /// </summary>
        public TextSpan NameSpan => new TextSpan(Span.Start, Name.FullName.Length);

        /// <summary>
        /// Does this attribute have a value component?
        /// </summary>
        public bool HasValue => Value != null
                    && Value.HasValue;

        public override string ToString()
        {
            return Name.ToString() + "=\"" + Value + "\"";
        }
    }
}

