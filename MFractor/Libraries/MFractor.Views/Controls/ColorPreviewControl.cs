using System;
using System.Collections.Generic;
using System.Drawing;
using MFractor.Utilities;
using Xwt;

namespace MFractor.Views.Controls
{
    public class ColorPreviewControl : Xwt.VBox
    {
        Canvas colorPreview;
        Label hexLabel;
        Label colorNameLabel;
        Label argbLabel;

        public byte Red
        {
            get => (byte)(colorPreview.BackgroundColor.Red * 255);
            set => ApplyColorValue(value, Green, Blue, Alpha);
        }

        public byte Green
        {
            get => (byte)(colorPreview.BackgroundColor.Green * 255);
            set => ApplyColorValue(Red, value, Blue, Alpha);
        }

        public byte Blue
        {
            get => (byte)(colorPreview.BackgroundColor.Blue * 255);
            set => ApplyColorValue(Red, Green, value, Alpha);
        }

        public byte Alpha
        {
            get => (byte)(colorPreview.BackgroundColor.Alpha * 255);
            set => ApplyColorValue(Red, Green, Blue, value);
        }

        public Color Color
        {
            get => Color.FromArgb(Alpha, Red, Green, Blue);
            set => ApplyColorValue(value);
        }

        IReadOnlyDictionary<string, Color> NamedColorMap { get; }


        public ColorPreviewControl()
            : this(Color.White)
        {
        }

        public ColorPreviewControl(Color color)
            : this(color.R, color.G, color.B, color.A)
        {
        }

        public ColorPreviewControl(byte red, byte green, byte blue, byte alpha)
        {
            NamedColorMap = ColorHelper.GetAllSystemDrawingColors();

            Build();

            ApplyColorValue(red, green, blue, alpha);
        }

        void ApplyColorValue(Color color)
        {
            ApplyColorValue(color.R, color.G, color.B, color.A);
        }

        void ApplyColorValue(byte red, byte green, byte blue, byte alpha)
        {
            colorPreview.BackgroundColor = Xwt.Drawing.Color.FromBytes(red, green, blue, alpha);
            hexLabel.Text = "HEX (ARGB): " + ToHex();
            argbLabel.Text = "RGBA: " + ToRGBA();

            if (GetMatchingColor(out var color))
            {
                colorNameLabel.Visible = true;
                colorNameLabel.Text = color.Name;
            }
            else
            {
                colorNameLabel.Visible = false;
            }
        }

        void Build()
        {
            colorPreview = new Canvas()
            {
                ExpandHorizontal = true,
                HeightRequest = 40,
                WidthRequest = 150,
            };
            PackStart(colorPreview);

            colorNameLabel = new Label();
            PackStart(colorNameLabel);

            hexLabel = new Label();
            PackStart(hexLabel);

            argbLabel = new Label();
            PackStart(argbLabel);
        }

        string ToHex()
        {
            return "#" + Alpha.ToString("X2") + Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
        }

        string ToRGBA()
        {
            return Red.ToString() + "," + Green.ToString() + "," + Blue.ToString() + "," + Alpha.ToString();
        }

        bool GetMatchingColor(out Color color)
        {
            color = Color.Transparent;

            foreach (var c in NamedColorMap)
            {
                if (Math.Abs(c.Value.R - Red) < double.Epsilon
                    && Math.Abs(c.Value.G - Green) < double.Epsilon
                    && Math.Abs(c.Value.B - Blue) < double.Epsilon)
                {
                    color = c.Value;
                    return true;
                }
            }

            return false;
        }

        // Name value (if applicable)
        // Color block
        // Hex Value.
        // A R G B 255 value
        // HSL Value
    }
}
