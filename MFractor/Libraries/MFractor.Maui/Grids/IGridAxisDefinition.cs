using System;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Grids
{
    public interface IGridAxisDefinition
    {
        /// <summary>
        /// The value of the size.
        /// </summary>
        string Value { get; }

        bool HasValue { get; }

        /// <summary>
        /// The span of the sizes <see cref="Value"/>.
        /// </summary>
        TextSpan? ValueSpan { get; }

        /// <summary>
        /// The full span of this element, including leading and trailing characters/whitespace.
        /// </summary>
        TextSpan FullSpan { get; }

        /// <summary>
        /// The format that this size element was defined in.
        /// </summary>
        GridAxisDefinitionFormat DefinitionFormat { get; }

        /// <summary>
        /// The name of the axis, such as RowDefinitions or ColumnDefinitions
        /// </summary>
        string Axis { get; }

        /// <summary>
        /// The name of the dimension for this axis, such as Width or height.
        /// </summary>
        string Dimension { get; }

        /// <summary>
        /// The name of this size.
        /// </summary>
        string Name { get; }

        bool HasName { get; }

        int Index { get; }

        XmlSyntax Syntax { get; }
    }
}