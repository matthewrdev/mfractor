using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Configuration.Attributes;
using MFractor.Maui.Xmlns;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IGridGenerator))]
    class GridGenerator : XamlCodeGenerator, IGridGenerator
    {
        public override string Documentation => "Generates a new `Grid` XAML element.";

        public override string Identifier => "com.mfractor.code_gen.xaml.xaml.grid";

        public override string Name => "Generate Grid";

        [ExportProperty("Should the new grid include a `ColumnDefinitions` element by default?")]
        public bool IncludeColumnDefinitions { get; set; } = true;

        [ExportProperty("Should the new grid include a `RowDefinitions` element by default?")]
        public bool IncludeRowDefinitions { get; set; } = true;

        [ExportProperty("What is the default amount of rows that the new grid should have?")]
        public int DefaultRowsCount { get; set; } = 1;

        [ExportProperty("What is the default amount of columns that the new grid should have?")]
        public int DefaultColumnsCount { get; set; } = 1;

        [Import]
        public IGridRowDefinitionGenerator GridRowDefinitionGenerator
        {
            get; set;
        }

        [Import]
		public IGridColumnDefinitionGenerator GridColumnDefinitionGenerator
		{
			get; set;
		}

        public XmlNode GenerateSyntax(IXamlFeatureContext context, IEnumerable<XmlNode> children)
        {
            var xmlns = context.Namespaces.ResolveNamespaceForSchema(context.Platform.SchemaUrl);
            var node = new XmlNode();
            node.Name = new XmlName(xmlns.Prefix, context.Platform.Grid.Name);

            if (IncludeColumnDefinitions)
            {
                var columns = new XmlNode();
                columns.Name = new XmlName(xmlns.Prefix, $"{context.Platform.Grid.Name}.{context.Platform.ColumnDefinitionsProperty}");

                for (var i = 0; i < DefaultColumnsCount; ++i)
                {
                    columns.AddChildNode(GridColumnDefinitionGenerator.GenerateSyntax(xmlns.Prefix));
                }

                node.AddChildNode(columns);
            }

            if (IncludeRowDefinitions)
            {
                var rows = new XmlNode();
                rows.Name = new XmlName(xmlns.Prefix, $"{context.Platform.Grid.Name}.{context.Platform.RowDefinitionsProperty}");

                for (var i = 0; i < DefaultRowsCount; ++i)
                {
                    rows.AddChildNode(GridRowDefinitionGenerator.GenerateSyntax(xmlns.Prefix));
                }

                node.AddChildNode(rows);
            }

            node.AddChildren(children);

            return node;
        }
    }
}
