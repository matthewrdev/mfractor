using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Editor.Adornments;
using MFractor.Editor.Utilities;
using MFractor.Maui.Grids;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.XAML.Adornments.Grids
{
    class GridIndexTagger : ITagger<GridIndexTag>
    {
        readonly string filePath;
        readonly IXmlSyntaxTreeService xmlSyntaxTreeService;
        readonly IXamlPlatformRepository xamlPlatforms;
        readonly IGridAxisResolver gridAxisResolver;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        class GridElement
        {
            public GridElement(Span span, int index, string sampleCode)
            {
                Span = span;
                Index = index;
                SampleCode = sampleCode;
            }

            public Span Span { get; }
            public int Index { get; }
            public string SampleCode { get; }

            public override string ToString()
            {
                return SampleCode;
            }
        }

        public GridIndexTagger(string filePath,
                               IXmlSyntaxTreeService xmlSyntaxTreeService,
                               IXamlPlatformRepository xamlPlatforms,
                               IGridAxisResolver gridAxisResolver) 
        {
            this.filePath = filePath;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
            this.xamlPlatforms = xamlPlatforms;
            this.gridAxisResolver = gridAxisResolver;
        }

        List<GridElement> DiscoverGridIndices(XmlNode node, IXamlPlatform platform)
        {
            var elements = new List<GridElement>();
            if (node == null)
            {
                return elements;
            }

            if (node == null)
            {
                return elements;
            }

            if (node.Name.FullName == platform.Grid.Name)
            {
                var rows = gridAxisResolver.ResolveRowDefinitions(node, platform).Where(r => r.DefinitionFormat == GridAxisDefinitionFormat.Node).ToList();
                var columns = gridAxisResolver.ResolveColumnDefinitions(node, platform).Where(c => c.DefinitionFormat == GridAxisDefinitionFormat.Node).ToList();

                if (rows.Any())
                {
                    foreach (var row in rows)
                    {
                        elements.Add(new GridElement(Span.FromBounds(row.FullSpan.Start, row.FullSpan.Start), row.Index, $"Grid.Row=\"{row.Index}\""));
                    }
                }

                if (columns.Any())
                {
                    foreach (var column in columns)
                    {
                        elements.Add(new GridElement(Span.FromBounds(column.FullSpan.Start, column.FullSpan.Start), column.Index, $"Grid.Column=\"{column.Index}\""));
                    }
                }
            }

            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    var innerElements = DiscoverGridIndices(child, platform);

                    if (innerElements != null && innerElements.Any())
                    {
                        elements.AddRange(innerElements);
                    }
                }
            }

            return elements;
        }

        public void Dispose()
        {
        }

        public IEnumerable<ITagSpan<GridIndexTag>> GetTags(NormalizedSnapshotSpanCollection spans)
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
            var platform = xamlPlatforms.ResolvePlatform(ast);
            if (platform is null)
            {
                yield break;
            }

            var gridElements = DiscoverGridIndices(ast?.Root, platform);

            foreach (var element in gridElements)
            {
                if (spans.Any(sp => sp.IntersectsWith(element.Span)))
                {
                    yield return new TagSpan<GridIndexTag>(new SnapshotSpan(snapshot, element.Span.Start, element.Span.Length), new GridIndexTag(element.Index, element.SampleCode));
                }
            }
        }
    }
}
