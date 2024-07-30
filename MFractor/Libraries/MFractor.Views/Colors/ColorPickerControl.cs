using System;
using System.Collections.Generic;
using System.Drawing;
using MFractor.Utilities;
using Xwt;

namespace MFractor.Views.Colors
{
    public class ColorPickerValueChangedEventArgs : EventArgs
    {
        public Color Color { get; }

        public ColorPickerValueChangedEventArgs(Color color)
        {
            Color = color;
        }
    }

    public class ColorPickerControl : Xwt.VBox
    {
        FrameBox colorPreview;
        TextEntry colorEntry;

        ColorChannelSelector redChannel;

        ColorChannelSelector greenChannel;

        ColorChannelSelector blueChannel;

        ColorChannelSelector alphaChannel;

        HBox colorValueContainer;
        Label colorLabel;

        public event EventHandler<ColorPickerValueChangedEventArgs> ColorValueChanged;

        public byte Red
        {
            get => redChannel.Value;
            set
            {
                redChannel.Value = value;
                ApplyColorChange();
                NotifyColorChanged();
            }
        }

        public byte Green
        {
            get => greenChannel.Value;
            set
            {
                alphaChannel.Value = value;
                ApplyColorChange();
                NotifyColorChanged();
            }
        }

        public byte Blue
        {
            get => blueChannel.Value;
            set
            {
                alphaChannel.Value = value;
                ApplyColorChange();
                NotifyColorChanged();
            }
        }

        public byte Alpha
        {
            get => alphaChannel.Value;
            set
            {
                alphaChannel.Value = value;

                ApplyColorChange();

                NotifyColorChanged();
            }
        }

        void ApplyColorChange()
        {
            if (GetMatchingColor(out var color) && color.IsNamedColor)
            {
                colorEntry.Text = color.Name;
            }
            else
            {
                colorEntry.Text = GetHexString();
            }

            colorPreview.BackgroundColor = Xwt.Drawing.Color.FromBytes(Red, Green, Blue, Alpha);
            colorPreview.QueueForReallocate();
        }

        public Color Color
        {
            get
            {
                if (GetMatchingColor(out var color))
                {
                    return color;
                }

                return Color.FromArgb(Alpha, Red, Green, Blue);
            }
            set
            {
                try
                {
                    UnbindEvents();

                    redChannel.Value = value.R;
                    greenChannel.Value = value.G;
                    blueChannel.Value = value.B;
                    alphaChannel.Value = value.A;

                    ApplyColorChange();

                    NotifyColorChanged();
                }
                finally
                {
                    BindEvents();
                }
            }
        }

        public IReadOnlyDictionary<string, Color> Colors { get; }

        public ColorPickerControl()
            : this(Color.Empty)
        {
        }

        public ColorPickerControl(Color color)
            : this(color.R, color.G, color.B, color.A)
        {
        }


        public ColorPickerControl(byte r, byte g, byte b)
            : this(r, g, b, byte.MaxValue)
        {
        }

        public ColorPickerControl(byte r, byte g, byte b, byte a)
        {
            Colors = ColorHelper.GetAllSystemDrawingColors();

            Build(r, g, b, a);

            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;

            colorEntry.Text = GetHexString();

            if (GetMatchingColor(out var color) && color.IsNamedColor)
            {
                colorEntry.Text = color.Name;
            }

            BindEvents();
        }

        bool GetMatchingColor(out Color color)
        {
            color = Color.Empty;

            foreach (var c in Colors)
            {
                if (Math.Abs(c.Value.R - Red) < double.Epsilon
                    && Math.Abs(c.Value.G - Green) < double.Epsilon
                    && Math.Abs(c.Value.B - Blue) < double.Epsilon
                    && Math.Abs(c.Value.A - Alpha) < double.Epsilon)
                {
                    color = c.Value;
                    return true;
                }
            }

            return false;
        }

        void Build(byte r, byte g, byte b, byte a)
        {
            colorPreview = new FrameBox()
            {
                ExpandHorizontal = true,
                HeightRequest = 60,
                WidthRequest = 250,
            };

            PackStart(colorPreview);

            PackStart(new HSeparator());

            colorValueContainer = new HBox();

            colorLabel = new Label
            {
                Text = "Color: ",
                Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold)
            };

            colorEntry = new TextEntry();

            colorValueContainer.PackStart(colorLabel);
            colorValueContainer.PackStart(colorEntry);

            PackStart(colorValueContainer);

            PackStart(new HSeparator());

            redChannel = new ColorChannelSelector("Red:  ", r, "Choose the red color channel.");
            greenChannel = new ColorChannelSelector("Green:", g, "Choose the green color channel.");
            blueChannel = new ColorChannelSelector("Blue: ", b, "Choose the blue color channel.");
            alphaChannel = new ColorChannelSelector("Alpha:", a, "Choose the alpha color channel.");

            PackStart(redChannel);
            PackStart(greenChannel);
            PackStart(blueChannel);
            PackStart(alphaChannel);
        }


        void NotifyColorChanged()
        {
            ColorValueChanged?.Invoke(this, new ColorPickerValueChangedEventArgs(Color));
        }

        void UnbindEvents()
        {
            redChannel.ChannelValueChanged -= ColorChannelValueChanged;
            greenChannel.ChannelValueChanged -= ColorChannelValueChanged;
            blueChannel.ChannelValueChanged -= ColorChannelValueChanged;
            alphaChannel.ChannelValueChanged -= ColorChannelValueChanged;
            colorEntry.Changed -= HexEntry_Changed;
        }

        void BindEvents()
        {
            UnbindEvents();

            redChannel.ChannelValueChanged += ColorChannelValueChanged;
            greenChannel.ChannelValueChanged += ColorChannelValueChanged;
            blueChannel.ChannelValueChanged += ColorChannelValueChanged;
            alphaChannel.ChannelValueChanged += ColorChannelValueChanged;
            colorEntry.Changed += HexEntry_Changed;
        }

        void HexEntry_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                if (ColorHelper.TryEvaluateColor(colorEntry.Text, 6, out var color))
                {
                    Color = color;
                }
            }
            finally
            {
                BindEvents();
            }
        }

        void ColorChannelValueChanged(object sender, ColorChannelValueChangedEventArgs e)
        {
            UnbindEvents();

            try
            {
                if (GetMatchingColor(out var color) && color.IsNamedColor)
                {
                    colorEntry.Text = color.Name;
                }
                else
                {
                    colorEntry.Text = GetHexString();
                }

                colorPreview.BackgroundColor = Xwt.Drawing.Color.FromBytes(Red, Green, Blue, Alpha);
                colorPreview.Hide();
                colorPreview.Show();

                NotifyColorChanged();
            }
            finally
            {
                BindEvents();
            }
        }

        public string GetHexString()
        {
            return Alpha.ToString("X2") + Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
        }

        public string GetRGBAString()
        {
            return Red.ToString() + "," + Green.ToString() + "," + Blue.ToString() + "," + Alpha.ToString();
        }
    }
}
