using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Text;
using MFractor.Utilities;
using Microsoft.Language.Xml;

namespace MFractor.Xml
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlSyntaxParser))]
    class XmlSyntaxParser : IXmlSyntaxParser
    {
        public XmlSyntaxTree ParseFile(FileInfo file)
        {
            if (file == null)
            {
                return default;
            }

            if (!file.Exists)
            {
                return null;
            }

            using (var stream = File.OpenRead(file.FullName))
            {
                return ParseStream(stream);
            }
        }

        public XmlSyntaxTree ParseStream(Stream stream)
        {
            if (stream == null)
            {
                return default;
            }

            return ParseText(stream.AsString());
        }

        public XmlSyntaxTree ParseFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return default;
            }

            return ParseFile(new FileInfo(filePath));
        }

        public XmlSyntaxTree ParseText(string content)
        {
            //using (Profiler.Profile())
            {
                var xmlDocumentSyntax = Parser.ParseText(content);

                var syntaxTree = Convert(xmlDocumentSyntax);

                syntaxTree.RawSyntax = xmlDocumentSyntax;

                return syntaxTree;
            }
        }

        public XmlSyntaxTree Convert(XmlDocumentSyntax syntax)
        {
            //using (Profiler.Profile())
            {
                var rootElement = syntax.RootSyntax as IXmlElementSyntax;

                if (rootElement is XmlElementSyntax rootElementSyntax
                    && rootElementSyntax.StartTag == null)// XML Header element?
                {
                    rootElement = rootElementSyntax.Elements.OfType<IXmlElementSyntax>().FirstOrDefault();
                }

                var rootSyntax = Convert(rootElement);

                var syntaxTree = new XmlSyntaxTree()
                {
                    Root = rootSyntax,
                };

                return syntaxTree;
            }
        }

        XmlNode Convert(IXmlElementSyntax syntax)
        {
            if (syntax == null || syntax.Name == null)
            {
                return null;
            }

            var node = new XmlNode
            {
                Name = Convert(syntax.NameNode)
            };

            SyntaxNode rawSyntax = null;

            if (syntax is XmlEmptyElementSyntax emptyElementSyntax)
            {
                rawSyntax = emptyElementSyntax;
                node.OpeningTagSpan = new Microsoft.CodeAnalysis.Text.TextSpan(emptyElementSyntax.Span.Start, emptyElementSyntax.Span.Length);
                node.IsSelfClosing = true;
                node.HasClosingTag = false;
            }
            else if (syntax is XmlElementSyntax elementSyntax)
            {
                rawSyntax = elementSyntax;

                node.IsSelfClosing = false;

                if (elementSyntax.StartTag != null)
                {
                    node.OpeningTagSpan = new Microsoft.CodeAnalysis.Text.TextSpan(elementSyntax.StartTag.Span.Start, elementSyntax.StartTag.Span.Length);
                }

                if (elementSyntax.EndTag != null)
                {
                    node.HasClosingTag = true;
                    node.ClosingTagSpan = new Microsoft.CodeAnalysis.Text.TextSpan(elementSyntax.EndTag.Span.Start, elementSyntax.EndTag.Span.Length);
                    node.ClosingTagName = Convert(elementSyntax.EndTag.NameNode);
                }

                var textSyntax = elementSyntax.Content.OfType<XmlTextSyntax>().FirstOrDefault();
                if (textSyntax != null)
                {
                    var token = textSyntax.TextTokens.OfType<SyntaxToken>().FirstOrDefault();
                    if (token != null)
                    {
                        node.Value = token.Text;
                    }
                }
            }

            if (syntax.Attributes != null)
            {
                foreach (var attr in syntax.Attributes)
                {
                    var attribute = Convert(attr);

                    if (attribute != null)
                    {
                        node.AddAttribute(attribute);
                    }
                }
            }

            if (syntax.Elements != null)
            {
                foreach (var c in syntax.Elements)
                {
                    var n = Convert(c);

                    if (n != null)
                    {
                        node.AddChildNode(n);
                    }
                }
            }

            node.RawSyntax = rawSyntax;
            return node;
        }

        XmlName Convert(XmlNameSyntax syntax)
        {
            if (syntax == null)
            {
                return null;
            }

            var xmlns = "";
            var localName = syntax.FullName;
            if (syntax.Prefix != null)
            {
                xmlns = syntax.Prefix;
            }

            if (syntax.LocalName != null)
            {
                localName = syntax.LocalName;
            }

            var name = new XmlName(xmlns, localName);
            name.RawSyntax = syntax;

            return name;
        }

        XmlAttribute Convert(XmlAttributeSyntax syntax)
        {
            if (syntax == null || syntax.NameNode == null)
            {
                return null;
            }

            var attribute = new XmlAttribute
            {
                Name = Convert(syntax.NameNode),
                Span = new Microsoft.CodeAnalysis.Text.TextSpan(syntax.Span.Start, syntax.Span.Length)
            };

            if (syntax.ValueNode != null)
            {
                attribute.Value = new XmlAttributeValue(syntax.Value)
                {
                    // MFractor uses the inner value span, the "+1" here removes the leading " character.
                    Span = new Microsoft.CodeAnalysis.Text.TextSpan(syntax.ValueNode.Span.Start + 1, syntax.Value.Length),
                    IsClosed = !string.IsNullOrEmpty(syntax.ValueNode.EndQuoteToken.Punctuation)
                };

                if (!attribute.Value.IsClosed)
                {
                }
            }

            attribute.RawSyntax = syntax;
            return attribute;
        }

        public XmlSyntaxTree ParseTextIncremental(string content, XmlSyntaxTree previous, IEnumerable<ITextReplacement> replacements)
        {
            //using (Profiler.Profile())
            {
                if (content is null)
                {
                    throw new System.ArgumentNullException(nameof(content));
                }

                if (previous is null)
                {
                    return ParseText(content);
                }

                if (replacements is null)
                {
                    throw new System.ArgumentNullException(nameof(replacements));
                }

                var oldDocument = previous.RawSyntax as XmlDocumentSyntax;

                var changed = replacements.Select(r => new TextChangeRange(new TextSpan(r.Offset, r.Length), r.Text?.Length ?? 0)).ToArray();

                var xmlDocumentSyntax = Parser.ParseIncremental(content, changed, oldDocument);

                var syntaxTree = Convert(xmlDocumentSyntax);

                syntaxTree.RawSyntax = xmlDocumentSyntax;

                return syntaxTree;
            }
        }

        public XmlSyntaxTree ParseText(ITextProvider textProvider)
        {
            if (textProvider is null)
            {
                return null;
            }

            return ParseText(textProvider.GetText());
        }
    }
}
