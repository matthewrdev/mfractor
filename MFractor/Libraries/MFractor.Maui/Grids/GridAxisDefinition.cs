using System;
using MFractor.Maui.Grids;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Grids
{
    class GridAxisDefinition : IGridAxisDefinition
    {        public GridAxisDefinition(string value,
                                  TextSpan? valueSpan,
                                  TextSpan fullSpan,
                                  int index,
                                  GridAxisDefinitionFormat definitionFormat,
                                  string axis,
                                  string dimension,
                                  string name)
        {
            Value = value ?? string.Empty;
            ValueSpan = valueSpan;
            FullSpan = fullSpan;
            Index = index;
            DefinitionFormat = definitionFormat;
            Axis = axis;
            Dimension = dimension;
            Name = name;
        }

        public string Value { get; }

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public TextSpan? ValueSpan { get; }

        public TextSpan FullSpan { get; }

        public GridAxisDefinitionFormat DefinitionFormat { get; }

        public string Axis  { get; }

        public string Dimension { get; }

        public string Name { get; }

        public bool HasName => !string.IsNullOrEmpty(Name);

        public XmlSyntax Syntax { get; set; }

        public int Index { get; }
    }
}