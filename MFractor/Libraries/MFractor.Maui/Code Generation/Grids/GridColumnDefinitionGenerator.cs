using System;
using System.ComponentModel.Composition;
using MFractor.Configuration.Attributes;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IGridColumnDefinitionGenerator))]
    class GridColumnDefinitionGenerator : XamlCodeGenerator, IGridColumnDefinitionGenerator
    {
        [ExportProperty("When creating a new `ColumnDefinition` for a grid, what is the default width value.")]
        public string DefaultWidthValue
        {
            get;
            set;
        } = "*";

        public override string Documentation => "Generates a `ColumnDefinition` Xaml node that is used inside a `Grid.ColumnDefinitions` node.";

        public override string Identifier => "com.mfractor.code_gen.xaml.xaml.grid_column_definition";

        public override string Name => "Generate Grid Column Definition";

        public XmlNode GenerateSyntax(string xmlns)
        {
            return GenerateSyntax(xmlns, DefaultWidthValue);
        }

        public XmlNode GenerateSyntax(string xmlns, string widthValue)
        {
            var node = new XmlNode();
            node.Name = new XmlName(xmlns, "ColumnDefinition");
            node.AddAttribute("Width", widthValue);
            node.IsSelfClosing = true;

            return node;
        }
    }
}
