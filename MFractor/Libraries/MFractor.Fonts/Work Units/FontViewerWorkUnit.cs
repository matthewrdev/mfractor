using System;
using MFractor.Work;

namespace MFractor.Fonts.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that launches the font viewer dialog.
    /// </summary>
    public class FontViewerWorkUnit : WorkUnit
    {
        /// <summary>
        /// The font to view.
        /// </summary>
        public IFont Font { get; set; }
    }
}