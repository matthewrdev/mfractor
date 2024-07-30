using System.Drawing;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;


namespace MFractor.Maui.Data.Models
{
    public class ColorUsage : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The literal value of this color.
        /// <para/>
        /// If the color is defined by properties instead of an inline
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The file offset for the start of the value section of the color declaration.
        /// </summary>
        /// <value>The name start.</value>
        public int ValueStart { get; set; }

        /// <summary>
        /// The file offset for the end of the name section of the color declaration.
        /// </summary>
        /// <value>The name end.</value>
        public int ValueEnd { get; set; }

        /// <summary>
        /// The span of the colors value area.
        /// </summary>
        /// <value>The span.</value>
        
        public TextSpan ValueSpan
        {
            get => TextSpan.FromBounds(ValueStart, ValueEnd);
            set
            {
                ValueStart = value.Start;
                ValueEnd = value.End;
            }
        }

        /// <summary>
        /// The color represented as a integer.
        /// <para/>
        /// Use <see cref="Color.FromArgb(int)"/> and <see cref="Color.ToArgb"/> to convert to and from this value.
        /// </summary>
        public int ColorInteger { get; set; }

        /// <summary>
        /// The color value of this <see cref="ColorDefinition"/>.
        /// </summary>
        
        public Color Color
        {
            get => Color.FromArgb(ColorInteger);
            set => ColorInteger = value.ToArgb();
        }


        /// <summary>
        /// Is this color a hexadecimal color?
        /// </summary>
        public bool IsHexColor { get; set; }
    }
}