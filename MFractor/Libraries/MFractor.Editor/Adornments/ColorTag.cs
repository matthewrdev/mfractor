using System;
using System.Drawing;
using MFractor.Work.WorkUnits;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.Adornments
{
    public class ColorTag : ITag
    {
        public Color Color { get; set; }

        public ColorEditedDelegate ColorEditedDelegate { get; set; }
    }
}
