using System;
using MFractor.Maui.FontSizes;

namespace MFractor.Maui.FontSizes
{
    class FontSize : IFontSize
    {
        public FontSize(string name, double iOS, double android, double uWP)
        {
            Name = name;
            IOS = iOS;
            Android = android;
            UWP = uWP;
        }

        public string Name { get; }

        public double IOS { get; }

        public double Android { get; }

        public double UWP { get; }
    }
}