using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.PlatformUI;
using Typography.OpenFont.Tables;

namespace MFractor.VS.Windows.Utilities
{
    public static class TextColorHelper
    {
        public static System.Windows.Media.Color GetThemeTextColor()
        {
            var textColorWpf = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            var textColor = new System.Windows.Media.Color()
            {
                A = textColorWpf.A,
                R = textColorWpf.R,
                G = textColorWpf.G,
                B = textColorWpf.B,
            };

            return textColor;
        }

        public static System.Windows.Media.Brush GetThemeTextBrush()
        {
            return new System.Windows.Media.SolidColorBrush(GetThemeTextColor());
        }
    }
}
