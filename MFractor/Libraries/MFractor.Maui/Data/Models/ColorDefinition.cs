using System.Drawing;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;


namespace MFractor.Maui.Data.Models
{
    /// <summary>
    /// A color static resource declaration.
    /// </summary>
    public class ColorDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The primary key of the <see cref="StaticResourceDefinition"/> that is this color.
        /// </summary>
        /// <value>The static resource identifier.</value>
        public int StaticResourceKey { get; set; }

        /// <summary>
        /// The name of this color resource.
        /// </summary>
        public string Name { get; set; }

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
        
        public TextSpan ValueSpan => TextSpan.FromBounds(ValueStart, ValueEnd);

        /// <summary>
        /// The red channel value.
        /// </summary>
        public byte Red { get; set; }

        /// <summary>
        /// The green channel value.
        /// </summary>
        public byte Green { get; set; }

        /// <summary>
        /// The blue channel value.
        /// </summary>
        public byte Blue { get; set; }

        /// <summary>
        /// The alpha channel value.
        /// </summary>
        public byte Alpha { get; set; }

        /// <summary>
        /// The color value of this <see cref="ColorDefinition"/>.
        /// </summary>
        
        public Color Color
        {
            get => Color.FromArgb(Alpha, Red, Green, Blue);
            set
            {
                Red = value.R;
                Green = value.G;
                Blue = value.B;
                Alpha = value.A;
            }
        }

        public int ColorInteger
        {
            get => Color.FromArgb(Alpha, Red, Green, Blue).ToArgb();
            set => Color = Color.FromArgb(value);
        }
    }
}
