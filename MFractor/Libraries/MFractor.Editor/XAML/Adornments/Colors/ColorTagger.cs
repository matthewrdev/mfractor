using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using MFractor.Data;
using MFractor.Editor.Adornments;
using MFractor.Editor.Utilities;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.XAML.Adornments.Colors
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(ColorTagger))]
    class ColorTagger : ITagger<ColorTag>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly ITextDocumentFactoryService textDocumentFactory;
        readonly IResourcesDatabaseEngine resourcesDatabaseEngine;
        readonly IXmlSyntaxTreeService xmlSyntaxTreeService;
        readonly IWorkEngine workEngine;

        List<ColorElement> elements;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        class ColorElement
        {
            public ColorElement(Span span, Color color)
            {
                Span = span;
                Color = color;
            }

            public Span Span { get; }

            public Color Color { get; }

            public ColorEditedDelegate EditColor { get; set; }
        }

        [ImportingConstructor]
        public ColorTagger(ITextDocumentFactoryService textDocumentFactory,
                           IResourcesDatabaseEngine resourcesDatabaseEngine,
                           IXmlSyntaxTreeService xmlSyntaxTreeService,
                           IWorkEngine workEngine)
        {
            this.textDocumentFactory = textDocumentFactory;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
            this.workEngine = workEngine;
        }

        List<ColorElement> DiscoverColors(string filePath, XmlNode node, IReadOnlyDictionary<string, Color> availableColorResources, TextSpan targetSpan)
        {
            var elements = new List<ColorElement>();
            if (node == null)
            {
                return elements;
            }

            if (node == null)
            {
                return elements;
            }

            if (node.HasAttributes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.HasValue && targetSpan.IntersectsWith(attribute.Value.Span))
                    {
                        if (attribute.Value.Value.Trim().StartsWith("#")
                            && ColorHelper.TryParseHexColor(attribute.Value.Value, out var color, out _))
                        {
                            elements.Add(new ColorElement(new Span(attribute.Value.Span.Start, attribute.Value.Span.Length), color)
                            {
                                EditColor = (result) =>
                                {
                                    workEngine.ApplyAsync(new ReplaceTextWorkUnit()
                                    {
                                        FilePath = filePath,
                                        Span = attribute.Value.Span,
                                        Text = ColorHelper.GetHexString(result, true),
                                    });
                                }
                            });
                        }
                        else if (attribute.Name.FullName.EndsWith("Color", StringComparison.InvariantCulture)
                                 && ColorHelper.TryEvaluateColor(attribute.Value.Value, out color))
                        {
                            elements.Add(new ColorElement(new Span(attribute.Value.Span.Start, attribute.Value.Span.Length), color)
                            {
                                EditColor = (result) =>
                                {
                                    workEngine.ApplyAsync(new ReplaceTextWorkUnit()
                                    {
                                        FilePath = filePath,
                                        Span = attribute.Value.Span,
                                        Text = ColorHelper.GetHexString(result, true),
                                    });
                                }
                            });
                        }
                        else if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
                        {
                            TryEvaluateColorResource(availableColorResources, elements, attribute);
                        }
                    }
                }
            }

            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    var innerElements = DiscoverColors(filePath, child, availableColorResources, targetSpan);

                    if (innerElements != null && innerElements.Any())
                    {
                        elements.AddRange(innerElements);
                    }

                    if (child.HasValue
                        && targetSpan.IntersectsWith(child.ValueSpan)
                        && child.Value.Trim().StartsWith("#")
                        && ColorHelper.TryParseHexColor(child.Value, out var color, out _))
                    {
                        elements.Add(new ColorElement(new Span(child.ValueSpan.Start, child.ValueSpan.Length), color)
                        {
                            EditColor = (result) =>
                            {
                                workEngine.ApplyAsync(new ReplaceTextWorkUnit()
                                {
                                    FilePath = filePath,
                                    Span = child.ValueSpan,
                                    Text = ColorHelper.GetHexString(result, true),
                                });
                            }
                        });
                    }
                }
            }

            return elements;
        }

        void TryEvaluateColorResource(IReadOnlyDictionary<string, Color> availableColorResources, List<ColorElement> elements, XmlAttribute attribute)
        {
            try
            {
                var parser = new XamlExpressionParser(attribute.Value.Value, attribute.Value.Span);

                var syntax = parser.Parse() as ExpressionSyntax;

                if (syntax != null)
                {
                    var name = string.Empty;
                    if (syntax.NameSyntax is SymbolSyntax symbol)
                    {
                        if (symbol.TypeNameSyntax != null)
                        {
                            name = symbol.TypeNameSyntax.Name;
                        }

                    }
                    else if (syntax.NameSyntax is TypeNameSyntax typeName)
                    {
                        name = typeName.Name;
                    }

                    if (name == "StaticResource")
                    {
                        var content = syntax.GetChild<ContentSyntax>();

                        if (content != null && !string.IsNullOrEmpty(content.Content) && availableColorResources.TryGetValue(content.Content, out var value))
                        {
                            elements.Add(new ColorElement(new Span(syntax.FullSpan.Start, syntax.FullSpan.Length), value));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void Dispose()
        {
        }

        public IEnumerable<ITagSpan<ColorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var snapshot = spans[0].Snapshot;

            var textBuffer = snapshot.TextBuffer;
            var project = TextBufferHelper.GetCompilationProject(textBuffer);

            if (!textDocumentFactory.TryGetTextDocument(textBuffer, out var textDocument))
            {
                yield break;
            }

            var ordered = spans.OrderBy(s => s.Start.Position);

            var start = ordered.First().Start.Position;
            var end = ordered.Last().End.Position;

            var ast = xmlSyntaxTreeService.GetSyntaxTree(textDocument.FilePath);

            var colors = new Dictionary<string, Color>();

            var database = resourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database != null && database.IsValid)
            {
                try
                {
                    var repo = database.GetRepository<ColorDefinitionRepository>();

                    var declaredColors = repo.GetAllDeclaredColors();

                    foreach (var declaredColor in declaredColors)
                    {
                        if (!colors.ContainsKey(declaredColor.Name))
                        {
                            colors[declaredColor.Name] = declaredColor.Color;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            elements = DiscoverColors(textDocument.FilePath, ast?.Root, colors, TextSpan.FromBounds(start, end));

            foreach (var element in elements)
            {
                if (spans.Any(sp => sp.IntersectsWith(element.Span)))
                {
                    var tag = new ColorTag()
                    {
                        Color = element.Color,
                        ColorEditedDelegate = element.EditColor,
                    };
                    var span = new SnapshotSpan(snapshot, element.Span.Start, element.Span.Length);

                    yield return new TagSpan<ColorTag>(span, tag);
                }
            }
        }
    }
}