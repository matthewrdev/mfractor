using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MFractor.Fonts;
using MFractor.VS.Windows.Utilities;

namespace MFractor.VS.Windows.Views
{
    class FontTextTooltipView : StackPanel
    {
        Label fontLabel;

        public FontTextTooltipView()
        {
            Orientation = Orientation.Vertical;
            Build();
        }

        public void SetFontText(IFont font, string text)
        {
            var filePath = "file://" + new FileInfo(font.FilePath).DirectoryName + "/#" + font.FamilyName;
            fontLabel.FontFamily = new System.Windows.Media.FontFamily(filePath);
            fontLabel.Content = text;
        }

        void Build()
        {
            fontLabel = new Label()
            {
                Foreground = TextColorHelper.GetThemeTextBrush(),
            };

            Children.Add(fontLabel);

            Children.Add(new Separator());
            Children.Add(new BrandedFooter());
        }
    }
}
