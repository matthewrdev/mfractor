using System;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Thickness
{
    public class ThicknessValue
    {
        public ThicknessDimension Dimension { get; set; }

        public double Value { get; set; }

        public TextSpan Span { get; set; }
    }
}
