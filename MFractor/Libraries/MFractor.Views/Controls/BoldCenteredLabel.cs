using System;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Controls
{
    public class BoldCenteredLabel : Label
    {
        public BoldCenteredLabel() => Initialize();

        public BoldCenteredLabel(string text)
        {
            Text = text;
            Initialize();
        }

        void Initialize()
        {
            TextAlignment = Alignment.Center;
            Font = Font.SystemFont
                .WithSize(10)
                .WithWeight(FontWeight.Bold);
        }
    }
}
