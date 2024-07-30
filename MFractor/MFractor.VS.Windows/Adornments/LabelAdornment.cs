using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MFractor.VS.Windows.Utilities;

namespace MFractor.VS.Windows.Adornments
{
    public class LabelAdornment : Label
    {
        public LabelAdornment() : base()
        {
        }

        public LabelAdornment(string character) : base()
        {
            Build();
            SetData(character);
        }

        void Build()
        {
            //
            // Setup UI
            FontFamily = new System.Windows.Media.FontFamily(UIHelpers.GetTextEditorFontFamilyName());
            FontSize = Math.Floor(UIHelpers.GetTextEditorFontSize() * 0.9);
            Foreground = UIHelpers.GetTextEditorPlainTextColor()
                .ToMediaColor()
                .ToBrush();
        }

        public void SetData(string character)
        {
            if (Content == null || (Content is string content && content != character))
            {
                Content = character;
            }
        }
    }
}
