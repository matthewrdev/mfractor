using System;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Controls
{
    public class CenteredLabel : Label
    {
        public CenteredLabel() => Initialize();

        public CenteredLabel(string text)
        {
            Text = text;
            Initialize();
        }

        void Initialize()
        {
            TextAlignment = Alignment.Center;
            Font = Font.SystemFont.WithSize(10);
        }
    }
}
