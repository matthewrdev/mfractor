using System.Drawing;
using MFractor.Navigation;

namespace MFractor.Editor.Tooltips
{
    public class ColorTooltipModel
    {
        public ColorTooltipModel(Color color,
                                 string label = "")
        {
            Color = color;
            Label = label;
        }

        public Color Color { get; }

        public string Label { get; }

        public string HelpUrl { get; set; }

        public INavigationContext NavigationContext { get; set; }

        public INavigationSuggestion NavigationSuggestion { get; set; }
    }
}
