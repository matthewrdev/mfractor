using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Grids;
using MFractor.Logging;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.CodeGeneration.Grids
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IInsertGridRowColumnGenerator))]
    class InsertGridRowColumnGenerator : XamlCodeGenerator, IInsertGridRowColumnGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.xaml.insert_grid_row_colum";

        public override string Name => "Insert Grid Row/Column Generator";

        public override string Documentation => "A code generator that inserts a grid row before or after a specific indice";

        readonly ILogger log = Logging.Logger.Create();

        [Import]
        public IGridAxisResolver GridAxisResolver { get; set; }

        public IReadOnlyList<IWorkUnit> InsertGridRow(IXamlPlatform platform, XmlNode grid, int insertionIndex, InsertionLocation insertionLocation, string filePath, string unit)
        {
            return InsertGridElement(platform,
                                     platform.RowProperty,
                                     platform.RowHeightProperty,
                                     platform.RowDefinitionsProperty,
                                     platform.RowDefinition.Name,
                                     grid,
                                     insertionIndex,
                                     insertionLocation,
                                     filePath,
                                     unit);
        }

        public IReadOnlyList<IWorkUnit> InsertGridColumn(IXamlPlatform platform, XmlNode grid, int insertionIndex, InsertionLocation insertionLocation, string filePath, string unit)
        {
            return InsertGridElement(platform,
                                     platform.ColumnProperty,
                                     platform.ColumnWidthProperty,
                                     platform.ColumnDefinitionsProperty,
                                     platform.ColumnDefinition.Name,
                                     grid,
                                     insertionIndex,
                                     insertionLocation,
                                     filePath,
                                     unit);
        }

        public IReadOnlyList<IWorkUnit> InsertGridElement(IXamlPlatform platform,
                                                        string elementName,
                                                        string dimensionName,
                                                        string definitionsPropertyName,
                                                        string definitionTypeName,
                                                        XmlNode grid,
                                                        int insertionIndex,
                                                        InsertionLocation insertionLocation,
                                                        string filePath,
                                                        string unit)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                throw new ArgumentException("message", nameof(elementName));
            }

            if (grid is null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            unit = unit ?? "Auto";

            var locationPropertyName = $"{platform.Grid.Name}.{elementName}";
            var spanPropertyName = $"{platform.Grid.Name}.{elementName}Span";

            var result = new List<IWorkUnit>();

            var format = GridAxisResolver.GetAxisDefinitionFormat(grid, definitionsPropertyName);
            var elements = GridAxisResolver.ResolveDefinitions(grid, definitionsPropertyName, platform)?.ToList();

            if (elements == null
                || !elements.Any()
                || format == GridAxisDefinitionFormat.Undefined)
            {
                log?.Warning($"The provided grid does not contain the {definitionsPropertyName} property");
                return Array.Empty<IWorkUnit>();
            }

            if (format == GridAxisDefinitionFormat.Attribute)
            {
                var insertion = insertionLocation == InsertionLocation.End ? $", {unit}" : $"{unit} ,";
                var location = insertionLocation == InsertionLocation.End ? elements[insertionIndex].FullSpan.End : elements[insertionIndex].FullSpan.Start;

                result.Add(new InsertTextWorkUnit(insertion, location, filePath));
            }
            else
            {
                var definitions = elements.Select(e => e.Syntax as XmlNode).ToList();

                if (!definitions.Any() || definitions.Count <= insertionIndex)
                {
                    log?.Warning($"The index {insertionIndex} is equal or greater than the amount of available {definitionsPropertyName} ({definitions?.Count ?? 0})");
                    return Array.Empty<IWorkUnit>();
                }

                var definition = definitions[insertionIndex];
                result.Add(new InsertXmlSyntaxWorkUnit()
                {
                    FilePath = filePath,
                    HostSyntax = definition.Parent,
                    AnchorSyntax = definition,
                    InsertionLocation = insertionLocation,
                    Syntax = new XmlNode(definitionTypeName)
                    {
                        Attributes = new List<XmlAttribute>()
                        {
                            new XmlAttribute(dimensionName, unit)
                        }
                    }
                });
            }

            // Update nodes.
            foreach (var child in grid.Children)
            {
                var elementSpan = child.GetAttributeByName(spanPropertyName);
                var elementLocation = child.GetAttributeByName(locationPropertyName);

                if (elementLocation == null
                    || !int.TryParse(elementLocation.Value?.Value, out var location))
                {
                    continue;
                }

                if (location >= insertionIndex)
                {
                    var isBeforeDesiredInsertion = location == insertionIndex && insertionLocation == InsertionLocation.End;
                    if (!isBeforeDesiredInsertion)
                    {
                        result.Add(new ReplaceTextWorkUnit()
                        {
                            FilePath = filePath,
                            Span = elementLocation.Value.Span,
                            Text = (location + 1).ToString()
                        });
                    }
                }

                if (elementSpan != null && int.TryParse(elementSpan.Value?.Value, out var span))
                {
                    var isBeforeDesiredInsertion = location == insertionIndex && insertionLocation == InsertionLocation.Start;
                    if (!isBeforeDesiredInsertion)
                    {
                        var end = location + span;

                        if (insertionIndex >= location && insertionIndex < end)
                        {
                            result.Add(new ReplaceTextWorkUnit()
                            {
                                FilePath = filePath,
                                Span = elementSpan.Value.Span,
                                Text = (span + 1).ToString()
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}