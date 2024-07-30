using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Grids;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;

namespace MFractor.Maui.Tooltips
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IGridTooltipRenderer))]
    class GridTooltipRenderer : IGridTooltipRenderer
    {
        const string defaultRowTooltip = "Could not resolve the row {0} in the parent grid.";
        const string defaultRowSpanTooltip = "Could not resolve the row span {0} in the parent grid.";
        const string defaultColumnTooltip = "Could not resolve the column {0} in the parent grid.";
        const string defaultColumnSpanTooltip = "Could not resolve the column span {0} in the parent grid.";

        readonly Lazy<IGridAxisResolver> gridAxisResolver;
        public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

        [ImportingConstructor]
        public GridTooltipRenderer(Lazy<IGridAxisResolver> gridAxisResolver)
        {
            this.gridAxisResolver = gridAxisResolver;
        }

        public string CreateRowTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform)
        {
            if (!int.TryParse(attribute.Value.Value, out var rowIndex))
            {
                return string.Format(defaultRowTooltip, attribute.Value.Value);
            }

            var rows = GridAxisResolver.ResolveRowDefinitions(gridNode, platform).ToList();
            if (!rows.Any())
            {
                return string.Format(defaultRowTooltip, attribute.Value.Value);
            }

            var index = rowIndex;

            if (index < 0 || index > rows.Count)
            {
                return string.Format(defaultRowTooltip, attribute.Value.Value);
            }

            var row = rows.FirstOrDefault(r => r.Index == index);
            if (row is null)
            {
                return string.Format(defaultRowTooltip, attribute.Value.Value);
            }

            var description = $"Using the {platform.RowProperty} at index {rowIndex} with the {platform.RowHeightProperty} of {row.Value}";

            if (row.HasName)
            {
                description += $" ({row.Name})";
            }

            return description;
        }

        public string CreateRowSpanTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform)
        {
            if (!int.TryParse(attribute.Value.Value, out var span))
            {
                return string.Format(defaultRowSpanTooltip, attribute.Value.Value);
            }

            var rows = GridAxisResolver.ResolveRowDefinitions(gridNode, platform).ToList();
            if (!rows.Any())
            {
                return string.Format(defaultRowSpanTooltip, attribute.Value.Value);
            }

            var rowName = $"{platform.Grid.Name}.{platform.RowProperty}";
            var rowAttribute = attribute.Parent.GetAttribute(a => a.Name.LocalName == rowName);

            if (!int.TryParse(rowAttribute?.Value?.Value, out var rowIndex))
            {
                return string.Format(defaultRowSpanTooltip, attribute.Value.Value);
            }

            var description = "Using a span of " + span.ToString() + " will cover these rows:" + Environment.NewLine;

            for (var i = rowIndex; i < rowIndex + span; ++i)
            {
                var row = rows.FirstOrDefault(r => r.Index == i);

                if (row is null)
                {
                    continue;
                }

                description += $" * {platform.RowProperty} {i.ToString()} - {platform.RowHeightProperty} \"{row.Value}\"{Environment.NewLine}";

                if (row.HasName)
                {
                    description += $" ({row.Name})";
                }
            }

            return description;
        }

        public string CreateColumnTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform)
        {
            if (!int.TryParse(attribute.Value.Value, out var columnIndex))
            {
                return string.Format(defaultColumnTooltip, attribute.Value.Value);
            }

            var columns = GridAxisResolver.ResolveColumnDefinitions(gridNode, platform).ToList();
            if (!columns.Any())
            {
                return string.Format(defaultColumnTooltip, attribute.Value.Value);
            }

            var index = columnIndex;

            if (index < 0 || index > columns.Count)
            {
                return string.Format(defaultColumnTooltip, attribute.Value.Value);
            }

            var column = columns.FirstOrDefault(r => r.Index == index);
            if (column is null)
            {
                return string.Format(defaultColumnTooltip, attribute.Value.Value);
            }

            var description = $"Using the {platform.ColumnProperty} at index {columnIndex} with the {platform.ColumnWidthProperty} of {column.Value}";

            if (column.HasName)
            {
                description += $" ({column.Name})";
            }

            return description;
        }

        public string CreateColumnSpanTooltip(XmlAttribute attribute, XmlNode gridNode, IXamlPlatform platform)
        {
            if (!int.TryParse(attribute.Value.Value, out var span))
            {
                return string.Format(defaultColumnSpanTooltip, attribute.Value.Value);
            }

            var columns = GridAxisResolver.ResolveColumnDefinitions(gridNode, platform).ToList();
            if (!columns.Any())
            {
                return string.Format(defaultColumnSpanTooltip, attribute.Value.Value);
            }

            var columnName = $"{platform.Grid.Name}.{platform.ColumnProperty}";
            var columnAttribute = attribute.Parent.GetAttribute(a => a.Name.LocalName == columnName);

            if (!int.TryParse(columnAttribute?.Value?.Value, out var columnIndex))
            {
                return string.Format(defaultColumnSpanTooltip, attribute.Value.Value);
            }

            var description = $"Using a span of {span} will cover these columns:{Environment.NewLine}";

            for (var i = columnIndex; i < columnIndex + span; ++i)
            {
                var column = columns.FirstOrDefault(c => c.Index == i);

                if (column is null)
                {
                    continue;
                }

                description += $" * {platform.ColumnProperty} {i} - {platform.ColumnWidthProperty} \"{column.Value}\"{Environment.NewLine}";

                if (column.HasName)
                {
                    description += $" ({column.Name})";
                }
            }

            return description;
        }
    }
}