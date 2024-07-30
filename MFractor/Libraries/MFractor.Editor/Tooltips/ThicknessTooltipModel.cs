using System;
using MFractor.Navigation;

namespace MFractor.Editor.Tooltips
{
    public class ThicknessTooltipModel
    {
        public ThicknessTooltipModel(double value)
            : this(value, value)
        {

        }

        public ThicknessTooltipModel(double horizontal, double vertical)
            : this (horizontal, horizontal, vertical, vertical)
        {
        }

        public ThicknessTooltipModel(double left, double right, double top, double bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public INavigationContext NavigationContext { get; set; }

        public INavigationSuggestion NavigationSuggestion { get; set; }

        public double Left { get; }
        public double Right { get; }
        public double Top { get; }
        public double Bottom { get; }
        public string Content { get; internal set; }
    }
}
