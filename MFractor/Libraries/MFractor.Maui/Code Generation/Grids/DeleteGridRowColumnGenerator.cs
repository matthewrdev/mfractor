using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.WorkUnits;
using MFractor.Logging;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDeleteGridRowColumnGenerator))]
    class DeleteGridRowColumnGenerator : XamlCodeGenerator, IDeleteGridRowColumnGenerator
    {
        readonly ILogger log = Logging.Logger.Create();

        public override string Identifier => "com.mfractor.code_gen.xaml.delete_grid_row_colum";

        public override string Name => "Delete Grid Row/Column Generator";

        public override string Documentation => "A code generator that delets a row or column in a grid, updating all childrens indices and spans";

        public IReadOnlyList<IWorkUnit> DeleteGridRow(IXamlPlatform platform, XmlNode grid, int deletionIndex, string filePath)
        {
            return DeleteGridElement(platform,
                                     platform.RowProperty,
                                     platform.RowDefinitionsProperty,
                                     platform.RowDefinition.Name,
                                     grid,
                                     deletionIndex,
                                     filePath);
        }

        public IReadOnlyList<IWorkUnit> DeleteGridColumn(IXamlPlatform platform, XmlNode grid, int deletionIndex, string filePath)
        {
            return DeleteGridElement(platform,
                                     platform.ColumnProperty,
                                     platform.ColumnDefinitionsProperty,
                                     platform.ColumnDefinition.Name,
                                     grid,
                                     deletionIndex,
                                     filePath);
        }

        public IReadOnlyList<IWorkUnit> DeleteGridElement(IXamlPlatform platform,
                                                        string elementName,
                                                        string definitionsPropertyName,
                                                        string definitionTypeName,
                                                        XmlNode grid,
                                                        int deletionIndex,
                                                        string filePath)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                throw new ArgumentException("message", nameof(elementName));
            }

            if (grid is null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            var locationPropertyName = $"{platform.Grid.Name}.{elementName}";
            var spanPropertyName = $"{platform.Grid.Name}.{elementName}Span";
            var definitionsName = $"{platform.Grid.Name}.{definitionsPropertyName}";
            var definitionName = definitionTypeName;

            var result = new List<IWorkUnit>();

            var definitionNode = grid.GetChildNode(definitionsName);

            if (definitionNode == null)
            {
                log?.Warning($"The provided grid does not contain the {definitionsName} property");
                return Array.Empty<IWorkUnit>();
            }

            var definitions = definitionNode.GetChildren(c => c.Name.FullName == definitionName);

            if (!definitions.Any() || definitions.Count <= deletionIndex)
            {
                log?.Warning($"The index {deletionIndex} is equal or greater than the amount of available {definitionsName} ({definitions?.Count ?? 0})");
                return Array.Empty<IWorkUnit>();
            }

            result.Add(new DeleteXmlSyntaxWorkUnit()
            {
                FilePath = filePath,
                Syntaxes = new List<XmlNode>() { definitions[deletionIndex] },
            });

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

                if (location >= deletionIndex && location > 0)
                {
                    result.Add(new ReplaceTextWorkUnit()
                    {
                        FilePath = filePath,
                        Span = elementLocation.Value.Span,
                        Text = (location - 1).ToString()
                    });
                }

                if (elementSpan != null && int.TryParse(elementSpan.Value?.Value, out var span))
                {
                    var end = location + span;

                    if (deletionIndex >= location && deletionIndex < end)
                    {
                        result.Add(new ReplaceTextWorkUnit()
                        {
                            FilePath = filePath,
                            Span = elementSpan.Value.Span,
                            Text = (span - 1).ToString()
                        });
                    }
                }
            }

            return result;
        }
    }
}