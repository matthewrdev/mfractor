using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlSyntaxFinder))]
    class XmlSyntaxFinder : IXmlSyntaxFinder
    {
        public XmlNode FindNodeAtOffset(XmlSyntaxTree document, int offset)
        {
            var path = BuildXmlPathToOffset(document, offset, out _);

            if (path == null || path.Any() == false)
            {
                return null;
            }

            XmlNode node = null;
            for (var i = path.Count; i > 0; i--)
            {
                var element = path[i - 1];

                if (element is XmlNode)
                {
                    node = element as XmlNode;
                    break;
                }
            }

            return node;
        }

        public XmlAttribute FindAttributeAtOffset(XmlSyntaxTree document, int offset)
        {
            var path = BuildXmlPathToOffset(document, offset, out _);

            if (path == null || path.Any() == false)
            {
                return null;
            }

            XmlAttribute attribute = null;
            for (var i = path.Count; i > 0; i--)
            {
                var element = path[i - 1];

                if (element is XmlAttribute)
                {
                    attribute = element as XmlAttribute;
                    break;
                }
            }

            return attribute;
        }

        public XmlSyntax GetXmlSyntaxAtOffset(XmlSyntaxTree syntaxTree, int offset, out TextSpan span)
        {
            span = default;

            var ended = false;

            XmlSyntax current = syntaxTree.Root;

            // Bad location.
            if (!FileLocationHelper.IsBetween(offset, syntaxTree.Root))
            {
                return null;
            }

            while (!ended)
            {
                if (current is XmlNode)
                {
                    var syntax = current as XmlNode;

                    if (syntax.IsSelfClosing)
                    {
                        if (!FileLocationHelper.IsBetween(offset, syntax))
                        {
                            break;
                        }

                        if (!syntax.HasAttributes)
                        {
                            var start = syntax.ClosingTagSpan.Start + 1;
                            var end = start + syntax.Name.FullName.Length;
                            span = TextSpan.FromBounds(start, end);
                            break;
                        }
                        else
                        {
                            XmlAttribute matchingAttribute = null;
                            foreach (var attr in syntax.Attributes)
                            {
                                if (FileLocationHelper.IsBetween(offset, attr))
                                {
                                    matchingAttribute = attr;
                                    break;
                                }
                            }

                            if (matchingAttribute != null)
                            {
                                current = matchingAttribute;
                                continue;
                            }
                            else
                            {
                                // ELement description.
                                var start = syntax.ClosingTagSpan.Start + 1;
                                var end = start + syntax.Name.FullName.Length;
                                span = TextSpan.FromBounds(start, end);
                                ended = true;
                            }
                        }
                    }
                    else
                    {
                        // Try search the attributes
                        if (syntax.OpeningTagSpan.IntersectsWith(offset))
                        {
                            if (syntax.Attributes == null || syntax.Attributes.Count() == 0)
                            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                                ended = true;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                                var start = syntax.OpeningTagSpan.Start + 1; // +1 to move start out of the '<' character.
                                var end = start + syntax.Name.FullName.Length;
                                span = TextSpan.FromBounds(start, end);
                                break;
                            }
                            else
                            {
                                XmlAttribute matchingAttribute = null;
                                foreach (var attr in syntax.Attributes)
                                {
                                    if (FileLocationHelper.IsBetween(offset, attr))
                                    {
                                        matchingAttribute = attr;
                                        break;
                                    }
                                }

                                if (matchingAttribute != null)
                                {
                                    current = matchingAttribute;
                                    continue;
                                }
                                else
                                {
                                    // ELement description.
                                    var start = syntax.ClosingTagSpan.Start + 1;
                                    var end = start + syntax.Name.FullName.Length;
                                    span = TextSpan.FromBounds(start, end);
                                    ended = true;
                                }
                            }
                        }
                        else if (syntax.ClosingTagSpan.IntersectsWith(offset))
                        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                            ended = true;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                            var start = syntax.ClosingTagSpan.Start + 2;// +2 to move out of the </ element
                            var end = syntax.ClosingTagSpan.End - 1; // -1 to move out of the > element
                            span = TextSpan.FromBounds(start, end);
                            break;
                        }
                        else if (FileLocationHelper.IsBetween(offset, syntax.OpeningTagSpan.End, syntax.ClosingTagSpan.Start))
                        {
                            XmlNode matchingElement = null;
                            if (syntax.Children != null)
                            {
                                foreach (var el in syntax.Children)
                                {
                                    if (FileLocationHelper.IsBetween(offset, el))
                                    {
                                        matchingElement = el;
                                        break;
                                    }
                                }
                            }

                            if (matchingElement != null)
                            {
                                current = matchingElement;
                                continue;
                            }
                            else
                            {
                                ended = true;
                            }

                        }
                        else
                        {
                            ended = true;
                        }
                    }
                }
                else if (current is XmlAttribute)
                {
                    var element = current as XmlAttribute;
                    ended = true;
                    span = element.Span;
                }
                else
                {
                    ended = true;
                }
            }

            return current;
        }

        public IReadOnlyList<XmlSyntax> BuildXmlPathToOffset(XmlSyntaxTree syntaxTree, int offset, out TextSpan span)
        {
            span = default;

            var path = new List<XmlSyntax>();

            var ended = false;

            XmlSyntax current = syntaxTree.Root;

            // Bad location.
            if (!FileLocationHelper.IsBetween(offset, syntaxTree.Root))
            {
                return new List<XmlSyntax>();
            }

            path.Add(current);

            while (!ended)
            {
                if (current is XmlNode)
                {
                    var syntax = current as XmlNode;

                    if (syntax.IsSelfClosing)
                    {
                        if (!FileLocationHelper.IsBetween(offset, syntax))
                        {
                            path.Clear();
                            break;
                        }

                        if (!syntax.HasAttributes)
                        {
                            var start = syntax.ClosingTagSpan.Start + 1;
                            var end = start + syntax.Name.FullName.Length;
                            span = TextSpan.FromBounds(start, end);
                            break;
                        }
                        else
                        {
                            var attrEnumerator = syntax.Attributes.GetEnumerator();
                            XmlAttribute matchingAttribute = null;
                            while (attrEnumerator.MoveNext())
                            {
                                if (FileLocationHelper.IsBetween(offset, attrEnumerator.Current))
                                {
                                    matchingAttribute = attrEnumerator.Current;
                                    break;
                                }
                            }

                            if (matchingAttribute != null)
                            {
                                current = matchingAttribute;
                                path.Add(current);
                                continue;
                            }
                            else
                            {
                                // ELement description.
                                var start = syntax.ClosingTagSpan.Start + 1;
                                var end = start + syntax.Name.FullName.Length;
                                span = TextSpan.FromBounds(start, end);
                                ended = true;
                            }
                        }
                    }
                    else
                    {
                        // Try search the attributes
                        if (syntax.OpeningTagSpan.IntersectsWith(offset))
                        {
                            if (!syntax.HasAttributes)
                            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                                ended = true;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                                var start = syntax.OpeningTagSpan.Start + 1; // +1 to move start out of the '<' character.
                                var end = start + syntax.Name.FullName.Length;
                                span = TextSpan.FromBounds(start, end);
                                break;
                            }
                            else
                            {
                                XmlAttribute matchingAttribute = null;
                                foreach (var attr in syntax.Attributes)
                                {
                                    if (FileLocationHelper.IsBetween(offset, attr))
                                    {
                                        matchingAttribute = attr;
                                        break;
                                    }
                                }

                                if (matchingAttribute != null)
                                {
                                    current = matchingAttribute;
                                    path.Add(current);
                                    continue;
                                }
                                else
                                {
                                    // ELement description.
                                    var start = syntax.ClosingTagSpan.Start + 1;
                                    var end = start + syntax.Name.FullName.Length;
                                    span = TextSpan.FromBounds(start, end);
                                    ended = true;
                                }
                            }
                        }
                        else if (syntax.ClosingTagSpan.IntersectsWith(offset))
                        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                            ended = true;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                            var start = syntax.ClosingTagSpan.Start + 2;// +2 to move out of the </ element
                            var end = syntax.ClosingTagSpan.End - 1; // -1 to move out of the > element
                            span = TextSpan.FromBounds(start, end);
                            break;
                        }
                        else if (FileLocationHelper.IsBetween(offset, syntax.OpeningTagSpan.End, syntax.ClosingTagSpan.Start))
                        {
                            XmlNode matchingElement = null;
                            if (syntax.Children != null)
                            {
                                foreach (var el in syntax.Children)
                                {
                                    if (FileLocationHelper.IsBetween(offset, el))
                                    {
                                        matchingElement = el;
                                        break;
                                    }
                                }
                            }

                            if (matchingElement != null)
                            {
                                current = matchingElement;
                                path.Add(current);
                                continue;
                            }
                            else if (syntax.HasValue && FileLocationHelper.IsBetween(offset, syntax.ValueSpan))
                            {
                                path.Add(syntax);
                                ended = true;
                            }
                            else
                            {
                                path.Clear();
                                ended = true;
                            }

                        }
                        else
                        {
                            path.Clear();
                            ended = true;
                        }
                    }
                }
                else if (current is XmlAttribute)
                {
                    var element = current as XmlAttribute;
                    ended = true;
                    span = element.Span;
                }
                else
                {
                    path.Clear();
                    ended = true;
                }
            }

            return path;
        }
    }
}
