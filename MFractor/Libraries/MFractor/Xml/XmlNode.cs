using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    /// <summary>
    /// Represent an XML node element in an XML syntax tree.
    /// </summary>
    public class XmlNode : XmlSyntax, IXmlNode
    {
        /// <summary>
        /// The parent of this XML node.
        /// </summary>
        /// <value>The parent.</value>
        public XmlNode Parent { get; set; }

        /// <summary>
        /// The name of this XML node.
        /// </summary>
        /// <value>The name.</value>
        public XmlName Name { get; set; }

        /// <summary>
        /// The name value of the closing tag of this XML node.
        /// <para/>
        /// When this node <see cref="IsSelfClosing"/> or <see cref="HasClosingTag"/> is <see langword="false"/>, this is null.
        /// </summary>
        /// <value>The name of the closing tag.</value>
        public XmlName ClosingTagName { get; set; }

        /// <summary>
        /// The inner value of this XML node.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// The attributes owned by this XML node.
        /// </summary>
        /// <value>The attributes.</value>
        public List<XmlAttribute> Attributes { get; set; }

        /// <summary>
        /// The child nodes of this XML node.
        /// </summary>
        /// <value>The children.</value>
        public List<XmlNode> Children { get; set; }

        /// <summary>
        /// Does the name tag of the XML node have closing syntax?
        /// </summary>
        /// <value><c>true</c> if is self closing; otherwise, <c>false</c>.</value>
        public bool IsSelfClosing { get; set; } = true;

        /// <summary>
        /// Does this XML node have a closing tag?
        /// </summary>
        /// <value><c>true</c> if has closing tag; otherwise, <c>false</c>.</value>
        public bool HasClosingTag { get; set; }

        /// <summary>
        /// The span of the opening tag of this XML node.
        /// </summary>
        /// <value>The opening tag span.</value>
        public TextSpan OpeningTagSpan { get; set; }

        /// <summary>
        /// The span of the closing tag of this XML node, including the leading </ element and the trailing > element.
        /// </summary>
        /// <value>The closing tag span.</value>
        public TextSpan ClosingTagSpan { get; set; }

        /// <summary>
        /// Is this XML node the top most element of the syntax tree?
        /// 
        /// Aka, does this XML node have a parent?
        /// </summary>
        /// <value><c>true</c> if is root; otherwise, <c>false</c>.</value>
        public bool IsRoot => Parent == null;

        /// <summary>
        /// Is this XML node the end of a branch in the syntax tree?
        /// 
        /// Aka, does this XML node have any children?
        /// </summary>
        /// <value><c>true</c> if is leaf; otherwise, <c>false</c>.</value>
        public bool IsLeaf => !HasChildren;

        /// <summary>
        /// Does this XML node have an inner value?
        /// </summary>
        /// <value><c>true</c> if has value; otherwise, <c>false</c>.</value>
        public bool HasValue => !string.IsNullOrEmpty(Value);

        /// <summary>
        /// Does this XML node have any attributes?
        /// </summary>
        /// <value><c>true</c> if has attributes; otherwise, <c>false</c>.</value>
        public bool HasAttributes => Attributes != null && Attributes.Count > 0;

        /// <summary>
        /// Does this XML node have any children?
        /// </summary>
        /// <value><c>true</c> if has children; otherwise, <c>false</c>.</value>
        public bool HasChildren => Children != null && Children.Count > 0;

        /// <summary>
        /// Does this XML node have a parent?
        /// </summary>
        /// <value><c>true</c> if has a parent; otherwise, <c>false</c>.</value>
        public bool HasParent => Parent != null;

        public XmlNode()
        {

        }

        public XmlNode(string name)
        {
            Name = new XmlName(name);
        }

        public bool ClosingTagNameSpanValid
        {
            get
            {
                if (!HasClosingTag)
                {
                    return true;
                }

                var start = ClosingTagSpan.Start + 2;
                var end = ClosingTagSpan.End - 1;

                if (end <= start)
                {
                    return false;
                }

                return true;
            }
        }

        public TextSpan ClosingTagNameSpan
        {
            get
            {
                if (!HasClosingTag)
                {
                    return TextSpan.FromBounds(0, 1);
                }

                var start = ClosingTagSpan.Start + 2;
                var end = ClosingTagSpan.End - 1;

                if (end <= start)
                {
                    return TextSpan.FromBounds(0, 1);
                }

                return TextSpan.FromBounds(start, end);
            }
        }

        /// <summary>
        /// Clones this XML node, optionally including children or attributes.
        /// </summary>
        public XmlNode Clone(bool includeChildren = true, bool includeAttributes = true)
        {
            var copy = new XmlNode()
            {
                Parent = Parent,
                Name = Name,
                Value = Value,
                ClosingTagName = ClosingTagName,
                IsSelfClosing = IsSelfClosing,
                HasClosingTag = HasClosingTag,
                OpeningTagSpan = OpeningTagSpan,
                ClosingTagSpan = ClosingTagSpan,
            };

            if (includeChildren && Children != null)
            {
                copy.Children = new List<XmlNode>(Children);
            }

            if (includeAttributes && Attributes != null)
            {
                copy.Attributes = new List<XmlAttribute>(Attributes);
            }

            return copy;
        }

        /// <summary>
        /// Does this XML node have an attribute named <paramref name="attributeName"/>?
        /// <para/>
        /// The <paramref name="attributeName"/> is the full name of the attribute, including the XML namespace component.
        /// </summary>
        public bool HasAttribute(string attributeName)
        {
            if (!HasAttributes)
            {
                return false;
            }

            return this.Attributes.Any(a => a.Name.FullName == attributeName);
        }

        /// <summary>
        /// Does this XML node have an attribute that matches the provided <paramref name="searchFunc"/>?
        /// </summary>
        public bool HasAttribute(Func<XmlAttribute, bool> searchFunc)
        {
            if (!HasAttributes)
            {
                return false;
            }

            return this.Attributes.Any(searchFunc);
        }

        /// <summary>
        /// Gets the first XML attribute that with the provided <paramref name="attributeName"/>.
        /// </summary>
        /// <returns>The attribute by name.</returns>
        /// <param name="attributeName">Attr name.</param>
        public XmlAttribute GetAttributeByName(string attributeName)
        {
            if (!HasAttributes)
            {
                return null;
            }

            return Attributes.FirstOrDefault(x => x.Name.FullName == attributeName);
        }

        /// <summary>
        /// Gets the attribute of this node by the loc
        /// </summary>
        /// <returns>The attribute by local name.</returns>
        /// <param name="localName">The local name of the attritube.</param>
        public XmlAttribute GetAttributeByLocalName(string localName)
        {
            if (!HasAttributes)
            {
                return null;
            }

            return Attributes.FirstOrDefault(x => x.Name.LocalName == localName);
        }

        public XmlNode GetChildAt(int index)
        {
            if (!HasChildren)
            {
                return null;
            }

            if (index >= Children.Count || index < 0)
            {
                return null;
            }

            return Children[index];
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="xmlns">Xmlns.</param>
        /// <param name="name">Name.</param>
        public XmlAttribute GetAttribute(string xmlns, string name)
        {
            if (!HasAttributes)
            {
                return null;
            }

            var fullName = name;

            if (!string.IsNullOrEmpty(xmlns))
            {
                fullName = xmlns + ":" + name;
            }

            return Attributes.FirstOrDefault(a => a.Name.FullName == fullName);
        }

        /// <summary>
        /// Gets the attribute using the provided <paramref name="searchFunc"/>.
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="searchFunc">Search func.</param>
		public XmlAttribute GetAttribute(Func<XmlAttribute, bool> searchFunc)
        {
            if (!HasAttributes)
            {
                return null;
            }

            return this.Attributes.FirstOrDefault(searchFunc);
        }

        /// <summary>
        /// Gets the attributes that match the provided <paramref name="searchFunc"/>.
        /// </summary>
        /// <returns>The attributes.</returns>
        /// <param name="searchFunc">Search func.</param>
        public IEnumerable<XmlAttribute> GetAttributes(Func<XmlAttribute, bool> searchFunc)
        {
            if (!HasAttributes)
            {
                return Enumerable.Empty<XmlAttribute>();
            }

            return Attributes.Where(searchFunc);
        }

        /// <summary>
        /// Adds the attribute to this XML node.
        /// </summary>
        /// <param name="attribute">Attribute.</param>
        public void AddAttribute(XmlAttribute attribute)
        {
            if (this.Attributes == null)
            {
                Attributes = new List<XmlAttribute>();
            }

            attribute.Parent = this;
            Attributes.Add(attribute);
        }

        /// <summary>
        /// Adds the attribute and value to this XML node.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public void AddAttribute(string name, string value)
        {
            AddAttribute(new XmlAttribute()
            {
                Name = new XmlName(name),
                Value = new XmlAttributeValue(value)
            });
        }

        /// <summary>
        /// Adds the <paramref name="child"/> to this XML node.
        /// </summary>
        /// <param name="child">Child.</param>
        public void AddChildNode(XmlNode child)
        {
            if (this.Children == null)
            {
                Children = new List<XmlNode>();
            }

            child.Parent = this;
            Children.Add(child);
        }

        /// <summary>
        /// Adds the <paramref name="children"/> to this XML node.
        /// </summary>
        /// <param name="children">Children.</param>
        public void AddChildren(IEnumerable<XmlNode> children)
        {
            if (this.Children == null)
            {
                Children = new List<XmlNode>();
            }

            Children.AddRange(children);
        }

        /// <summary>
        /// Searches this XML node for a child with the <paramref name="fullName"/>.
        /// </summary>
        /// <returns><c>true</c>, if child named was hased, <c>false</c> otherwise.</returns>
        /// <param name="fullName">Full name.</param>
        public bool HasChildNamed(string fullName)
        {
            if (!HasChildren)
            {
                return false;
            }

            return Children.Any(c => c.Name.FullName == fullName);
        }

        /// <summary>
        /// Searches this XML node for the provided <paramref name="node"/>.
        /// </summary>
        /// <returns><c>true</c>, if child was hased, <c>false</c> otherwise.</returns>
        /// <param name="node">Node.</param>
        public bool HasChild(XmlNode node)
        {
            if (!HasChildren)
            {
                return false;
            }

            return Children.Contains(node);
        }

        /// <summary>
        /// Searches this XML node for any children matching <paramref name="searchFunc"/>.
        /// </summary>
        /// <returns><c>true</c>, if child was hased, <c>false</c> otherwise.</returns>
        /// <param name="searchFunc">Search func.</param>
        public bool HasChild(Func<XmlNode, bool> searchFunc)
        {
            if (!HasChildren)
            {
                return false;
            }

            return Children.Any(searchFunc);
        }

        /// <summary>
        /// Gets the child node for this XML node with the namespace <paramref name="xmlns"/> and name of <paramref name="localName"/>.
        /// </summary>
        /// <returns>The child node.</returns>
        /// <param name="xmlns">Xmlns.</param>
        /// <param name="localName">Local name.</param>
        public XmlNode GetChildNode(string xmlns, string localName)
        {
            if (!HasChildren)
            {
                return null;
            }

            var fullName = string.IsNullOrEmpty(xmlns) ? localName : $"{xmlns}:{localName}";

            return GetChildNode(fullName);
        }

        /// <summary>
        /// Gets the child node of this XML node that matches <paramref name="fullName"/>.
        /// </summary>
        /// <returns>The child node.</returns>
        /// <param name="fullName">Full name.</param>
        public XmlNode GetChildNode(string fullName)
        {
            if (!HasChildren)
            {
                return null;
            }

            return Children.FirstOrDefault(c => c.Name.FullName == fullName);
        }

        /// <summary>
        /// Gets the first child node that matches <paramref name="searchFunc"/>
        /// </summary>
        /// <returns>The child node.</returns>
        /// <param name="searchFunc">Search func.</param>
        public XmlNode GetChildNode(Func<XmlNode, bool> searchFunc)
        {
            if (!HasChildren)
            {
                return null;
            }

            return Children.FirstOrDefault(searchFunc);
        }

        /// <summary>
        /// Gets all the children of this XML node that match <paramref name="searchFunc"/>.
        /// </summary>
        /// <returns>The children.</returns>
        /// <param name="searchFunc">Search func.</param>
        public List<XmlNode> GetChildren(Func<XmlNode, bool> searchFunc)
        {
            if (!HasChildren)
            {
                return new List<XmlNode>();
            }

            return Children.Where(searchFunc).ToList();
        }

        /// <summary>
        /// The span of the inner value content.
        /// </summary>
        /// <value>The value span.</value>
        public TextSpan ValueSpan
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException("Cannot retrieve the value region for an XmlNode without a value");
                }

                if (IsSelfClosing)
                {
                    throw new InvalidOperationException("Cannot retrieve the value region for an XmlNode that is self closing: it cannot have a value");
                }

                return new TextSpan(OpeningTagSpan.End, Value.Length);
            }
        }

        /// <summary>
        /// The span of this entire XML node.
        /// </summary>
        /// <value>The span.</value>
        public TextSpan Span
        {
            get
            {
                if (IsSelfClosing)
                {
                    return OpeningTagSpan;
                }

                return TextSpan.FromBounds(OpeningTagSpan.Start, ClosingTagSpan.End);
            }
        }


        /// <example><para>The following example outputs the textual description of
        /// the value of an object of type <see cref="System.Object" /> to the console.</para><code lang="C#">using System;
        /// 
        /// class MyClass {
        /// static void Main() {
        /// object o = new object();
        /// Console.WriteLine (o.ToString());
        /// }
        /// }
        /// </code><para>The output is</para><para><c>System.Object</c></para></example><remarks><attribution license="cc4" from="Microsoft" modified="false" /><para><see cref="M:System.Object.ToString" /> is the major formatting method in the .NET Framework. It converts an object to its string representation so that it is suitable for display. (For information about formatting support in the .NET Framework, see <format type="text/html"><a href="0d1364da-5b30-4d42-8e6b-03378343343f">Formatting Types</a></format>.) </para><para>The default implementation of the <see cref="M:System.Object.ToString" /> method returns the fully qualified name of the type of the <see cref="System.Object" />, as the following example shows.</para><para>code reference: System.Object.ToString#1</para><para>Because <see cref="System.Object" /> is the base class of all reference types in the .NET Framework, this behavior is inherited by reference types that do not override the <see cref="M:System.Object.ToString" /> method. The following example illustrates this. It defines a class named Object1 that accepts the default implementation of all <see cref="System.Object" /> members. Its <see cref="M:System.Object.ToString" /> method returns the object's fully qualified type name.</para><para>code reference: System.Object.ToString#2</para><para>Types commonly override the <see cref="M:System.Object.ToString" /> method to return a string that represents the object instance. For example, the base types such as <see cref="System.Char" />, <see cref="System.Int32" />, and <see cref="System.String" /> provide <see cref="M:System.Object.ToString" /> implementations that return the string form of the value that the object represents. The following example defines a class, Object2, that overrides the <see cref="M:System.Object.ToString" /> method to return the type name along with its value.</para><para>code reference: System.Object.ToString#3</para><format type="text/html"><h2>Notes for the wrt</h2></format><para>When you call the <see cref="M:System.Object.ToString" /> method on a class in the wrt, it provides the default behavior for classes that don’t override <see cref="M:System.Object.ToString" />. This is part of the support that the .NET Framework provides for the wrt (see <format type="text/html"><a href="6fa7d044-ae12-4c54-b8ee-50915607a565">.NET Framework Support for Windows Store Apps and Windows Runtime</a></format>). Classes in the wrt don’t inherit <see cref="System.Object" />, and don’t always implement a <see cref="M:System.Object.ToString" />. However, they always appear to have <see cref="M:System.Object.ToString" />, <see cref="M:System.Object.Equals(System.Object)" />, and <see cref="M:System.Object.GetHashCode" /> methods when you use them in your C# or Visual Basic code, and the .NET Framework provides a default behavior for these methods. </para><para>Starting with the net_v451, the common language runtime will use <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see> on a wrt object before falling back to the default implementation of <see cref="M:System.Object.ToString" />. </para><block subset="none" type="note"><para>wrt classes that are written in C# or Visual Basic can override the <see cref="M:System.Object.ToString" /> method. </para></block><format type="text/html"><h2>The wrt and the IStringable Interface</h2></format><para>Starting with win81, the wrt includes an <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> interface whose single method, <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see>, provides basic formatting support comparable to that provided by <see cref="M:System.Object.ToString" />. To prevent ambiguity, you should not implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> on managed types. </para><para>When managed objects are called by native code or by code written in languages such as JavaScript or C++/CX, they appear to implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see>. The common language runtime will automatically route calls from <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see> to <see cref="M:System.Object.ToString" /> in the event <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> is not implemented on the managed object. </para><block subset="none" type="note"><para>Because the common language runtime auto-implements <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> for all managed types in win8_appstore_long apps, we recommend that you do not provide your own <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> implementation. Implementing <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> may result in unintended behavior when calling ToString  from the wrt, C++/CX, or JavaScript. </para></block><para>If you do choose to implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> in a public managed type that is exported in a wrt component, the following restrictions apply: </para><list type="bullet"><item><para>You can define the <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> interface only in a "class implements" relationship, such as </para><code>public class NewClass : IStringable</code><para>in C#, or</para><code>Public Class NewClass : Implements IStringable</code><para>in Visual Basic. </para></item><item><para>You cannot implement <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> on an interface. </para></item><item><para>You cannot declare a parameter to be of type <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see>. </para></item><item><para><see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> cannot be the return type of a method, property, or field. </para></item><item><para>You cannot hide your <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> implementation from base classes by using a method definition such as the following:  </para><code>
        /// public class NewClass : IStringable
        /// {
        /// public new string ToString()
        /// {
        /// return "New ToString in NewClass";
        /// }
        /// }
        /// </code><para>Instead, the <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">IStringable.ToString</see> implementation must always override the base class implementation. You can hide a ToString implementation only by invoking it on a strongly typed class instance. </para></item></list><para>Note that under a variety of conditions, calls from native code to a managed type that implements <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.aspx">IStringable</see> or hides its <see cref="http://msdn.microsoft.com/library/windows/apps/windows.foundation.istringable.tostring.aspx">ToString</see> implementation can produce unexpected behavior. </para></remarks><summary><attribution license="cc4" from="Microsoft" modified="false" /><para>Returns a string that represents the current object.</para></summary><returns><attribution license="cc4" from="Microsoft" modified="false" /><para>A string that represents the current object.</para></returns>
        public override string ToString()
        {
            return Name.ToString();
        }

        /// <summary>
        /// The span of the name element of this XML node.
        /// </summary>
        /// <value>The name span.</value>
        public TextSpan NameSpan => new TextSpan(OpeningTagSpan.Start + 1, Name.FullName.Length);
    }
}

