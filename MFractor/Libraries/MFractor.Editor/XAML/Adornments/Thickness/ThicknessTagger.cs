using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Editor.Adornments;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.XAML.Adornments.Thickness
{
    class ThicknessTagger : ITagger<ThicknessTag>
    {
        readonly string filePath;
        readonly IXmlSyntaxTreeService xmlSyntaxTreeService;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        class ThicknessValue
        {
            public ThicknessValue(Span span, string orientation)
            {
                Span = span;
                Orientation = orientation;
            }

            public Span Span { get; }
            public string Orientation { get; }

            public override string ToString()
            {
                return Orientation;
            }
        }

        public ThicknessTagger(string filePath, IXmlSyntaxTreeService xmlSyntaxTreeService)
        {
            this.filePath = filePath;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
        }
        
        List<ThicknessValue> DiscoverThicknesses(XmlNode node)
        {
            var elements = new List<ThicknessValue>();
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
                    var innerElements = DiscoverThicknesses(child);

                    if (innerElements != null && innerElements.Any())
                    {
                        elements.AddRange(innerElements);
                    }
                }
            }

            if (node.HasValue && node.Name.FullName == "Thickness")
            {
                elements.AddRange(ProcessValue(node.Value, node.ValueSpan));
            }

            if (node.HasAttributes)
            {
                var margin = node.GetAttributeByName("Margin");
                if (margin != null && margin.HasValue)
                {
                    elements.AddRange(ProcessValue(margin.Value.Value, margin.Value.Span));
                }

                var padding = node.GetAttributeByName("Padding");
                if (padding != null && padding.HasValue)
                {
                    elements.AddRange(ProcessValue(padding.Value.Value, padding.Value.Span));
                }
            }

            return elements;
        }

        IReadOnlyList<ThicknessValue> ProcessValue(string value, Microsoft.CodeAnalysis.Text.TextSpan valueSpan)
        {
            if (string.IsNullOrEmpty(value)
                || ExpressionParserHelper.IsExpression(value))
            {
                return new List<ThicknessValue>();
            }

            if (!ThicknessHelper.ProcessThickness(value, valueSpan, out var thicknessValues))
            {
                return new List<ThicknessValue>();
            }

            return thicknessValues.Select(tv => new ThicknessValue(new Span(tv.Span.Start, 0), GetCharacterForOrientation(tv.Dimension))).ToList();
        }

        string GetCharacterForOrientation(Maui.Thickness.ThicknessDimension dimension)
        {
            switch (dimension)
            {
                case Maui.Thickness.ThicknessDimension.Left:
                    return "←";
                case Maui.Thickness.ThicknessDimension.Right:
                    return "→";
                case Maui.Thickness.ThicknessDimension.Top:
                    return "↑";
                case Maui.Thickness.ThicknessDimension.Bottom:
                    return "↓";
                case Maui.Thickness.ThicknessDimension.Horizontal:
                    return "⬌";
                case Maui.Thickness.ThicknessDimension.Vertical:
                    return "⬍";
                default:
                case Maui.Thickness.ThicknessDimension.All:
                    return "✣";
            }
        }

        public void Dispose()
        {
        }

        public IEnumerable<ITagSpan<ThicknessTag>> GetTags(NormalizedSnapshotSpanCollection spans)
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

            var thicknesses = DiscoverThicknesses(ast?.Root);

            foreach (var element in thicknesses)
            {
                if (spans.Any(sp => sp.IntersectsWith(element.Span)))
                {
                    yield return new TagSpan<ThicknessTag>(new SnapshotSpan(snapshot, element.Span.Start, element.Span.Length), new ThicknessTag { Orientation = element.Orientation });
                }
            }
        }
    }
}
