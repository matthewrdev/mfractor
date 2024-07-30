using System.ComponentModel.Composition;
using MFractor.Configuration.Attributes;
using MFractor.Xml;

namespace MFractor.Maui.CodeGeneration.Grids
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IGridRowDefinitionGenerator))]
    class GridRowDefinitionGenerator : XamlCodeGenerator, IGridRowDefinitionGenerator
    {
        [ExportProperty("When creating a new `RowDefinition` for a grid, what is the default height of a newly created row?")]
        public string DefaultHeightValue
        {
            get;
            set;
        } = "*";

        public override string Documentation => "Generates a `RowDefinition` Xaml node that is used inside a `Grid.RowDefinitions` node.";

        public override string Identifier => "com.mfractor.code_gen.xaml.xaml.grid_row_definition";

        public override string Name => "Generate Grid Row Definition";

        public XmlNode GenerateSyntax(string xmlns)
        {
            return GenerateSyntax(xmlns, DefaultHeightValue);
        }

        public XmlNode GenerateSyntax(string xmlns, string heightValue)
        {
            var node = new XmlNode();
            node.Name = new XmlName(xmlns, "RowDefinition");
            node.AddAttribute("Height", heightValue);
            node.IsSelfClosing = true;

            return node;
        }
    }
}
