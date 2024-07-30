using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MFractor.Editor.Adornments;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.XAML.Adornments.EscapedXamlCharacters
{
    class EscapedXamlCharacterTagger : ITagger<EscapedCharacterTag>
    {
        readonly string filePath;
        readonly IXmlSyntaxTreeService xmlSyntaxTreeService;

        List<EscapedXamlCharacter> characters;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        class EscapedXamlCharacter
        {
            public EscapedXamlCharacter(Span span, string character)
            {
                Span = span;
                Character = character;
            }

            public Span Span { get; }
            public string Character { get; }

            public override string ToString()
            {
                return Character;
            }
        }

        public EscapedXamlCharacterTagger(string filePath,
                               IXmlSyntaxTreeService xmlSyntaxTreeService)
        {
            this.filePath = filePath;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
        }

        readonly Regex escapedCharactersRegex = new Regex("(&gt;)|(&lt;)|(&amp;)|(&quot;)|(&#10;)", RegexOptions.Compiled);

        List<EscapedXamlCharacter> DiscoverEscapedXamlCharacters(XmlNode node)
        {
            var elements = new List<EscapedXamlCharacter>();
            if (node == null)
            {
                return elements;
            }

            if (node == null)
            {
                return elements;
            }

            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    var innerElements = DiscoverEscapedXamlCharacters(child);

                    if (innerElements != null && innerElements.Any())
                    {
                        elements.AddRange(innerElements);
                    }
                }
            }

            if (node.HasValue && node.Value.Contains("&"))
            {
                var matches = escapedCharactersRegex.Matches(node.Value);

                if (matches != null && matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        elements.Add(new EscapedXamlCharacter(new Span(node.ValueSpan.Start + match.Index, match.Length), match.Value));
                    }
                }
            }

            if (node.HasAttributes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.HasValue && attribute.Value.Value.Contains("&"))
                    {
                        var matches = escapedCharactersRegex.Matches(attribute.Value.Value);

                        if (matches != null && matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                elements.Add(new EscapedXamlCharacter(new Span(attribute.Value.Span.Start + match.Index, match.Length), match.Value));
                            }
                        }
                    }
                }
            }

            return elements;
        }

        public void Dispose()
        {
        }

        public IEnumerable<ITagSpan<EscapedCharacterTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var ordered = spans.OrderBy(s => s.Start.Position);

            var start = ordered.First().Start.Position;
            var end = ordered.Last().End.Position;

            var snapshot = spans[0].Snapshot;

            var ast = xmlSyntaxTreeService.GetSyntaxTree(filePath);

            characters = DiscoverEscapedXamlCharacters(ast?.Root);

            foreach (var element in characters)
            {
                if (spans.Any(sp => sp.IntersectsWith(element.Span)))
                {
                    yield return new TagSpan<EscapedCharacterTag>(new SnapshotSpan(snapshot, element.Span.Start, element.Span.Length), new EscapedCharacterTag(element.Character));
                }
            }
        }
    }
}
