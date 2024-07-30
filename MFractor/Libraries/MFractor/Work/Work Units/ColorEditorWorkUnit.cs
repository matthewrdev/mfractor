using System;
using System.Drawing;

namespace MFractor.Work.WorkUnits
{
    public delegate void ColorEditedDelegate(Color color);

    /// <summary>
    /// An <see cref="IWorkUnit"/> that shows a color picker input.
    /// </summary>
    public class ColorEditorWorkUnit : WorkUnit
    {
        /// <summary>
        /// The color for the color picker.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// The callback to invoke after the color has been edited.
        /// </summary>
        public ColorEditedDelegate ColorEditedDelegate { get; }

        public ColorEditorWorkUnit(Color defaultColor,
                                   ColorEditedDelegate colorEditedDelegate)
        {
            Color = defaultColor;
            ColorEditedDelegate = colorEditedDelegate ?? throw new ArgumentNullException(nameof(colorEditedDelegate));
        }
    }
}
