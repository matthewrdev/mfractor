using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for working with colors.
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// Compares the <paramref name="left"/> and <paramref name="right"/> colors to see if they closely match.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static bool ColorsAreClose(Color left, Color right, int threshold = 10)
        {
            int red = (int)left.R - right.R,
                green = (int)left.G - right.G,
                blue = (int)left.B - right.B,
                alpha = (int)left.A - right.A;
            return (red * red + green * green + blue * blue + alpha * alpha) <= threshold * threshold;
        }

        static readonly Lazy<IReadOnlyDictionary<string, Color>> namedColorMap = new Lazy<IReadOnlyDictionary<string, Color>>(GetAllSystemDrawingColors);
        static IReadOnlyDictionary<string, Color> NamedColorMap => namedColorMap.Value;

        /// <summary>
        /// Inspects the <see cref="Color"/> class and returns all color constants.
        /// </summary>
        /// <returns>The all system drawing colors.</returns>
        public static IReadOnlyDictionary<string, Color> GetAllSystemDrawingColors()
        {
            var results = new Dictionary<string, Color>();
            var colorType = typeof(System.Drawing.Color);
            // We take only static property to avoid properties like Name, IsSystemColor ...
            var propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (var propInfo in propInfos)
            {
                results[propInfo.Name] = (Color)propInfo.GetValue(null, null);
            }

            return results;
        }


        /// <summary>
        /// Render the provided <paramref name="colorValue"/> into a bitmap and export it to <paramref name="colorFilePath"/>.
        /// </summary>
        /// <param name="colorFilePath">Color file path.</param>
        /// <param name="colorValue">Color value.</param>
        public static void RenderColorBitmap(string colorFilePath, System.Drawing.Color colorValue)
        {
            //a holder for the result
            using (var result = new System.Drawing.Bitmap(16, 16))
            {
                //use a graphics object to draw the resized image into the bitmap
                using (var graphics = System.Drawing.Graphics.FromImage(result))
                {
                    //set the resize quality modes to high quality
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    //draw the image into the target bitmap
                    graphics.Clear(System.Drawing.Color.Transparent);
                    using (var b = new System.Drawing.SolidBrush(colorValue))
                    {
                        graphics.FillEllipse(b, 2, 2, 12, 12);
                    }

                    using (var p = new System.Drawing.Pen(System.Drawing.Color.SlateGray, 1))
                    {
                        graphics.DrawEllipse(p, 2, 2, 12, 12);
                    }

                    result.Save(colorFilePath);
                }
            }
        }

        /// <summary>
        /// Given the <paramref name="value"/>, try to convert it into a new <see cref="System.Drawing.Color"/>.
        /// </summary>
        /// <returns><c>true</c>, if evaluate color was tryed, <c>false</c> otherwise.</returns>
        /// <param name="value">Value.</param>
        /// <param name="color">Color.</param>
        public static bool TryEvaluateColor(string value, out System.Drawing.Color color)
        {
            return TryEvaluateColor(value, 3, out color);
        }

        public static bool TryEvaluateColor(string value, int minimumHexLength, out Color color)
        {
            color = Color.Empty;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (TryParseHexColor(value, minimumHexLength, out color, out var hasAlpha))
            {
                return true;
            }

            try
            {
                color = System.Drawing.ColorTranslator.FromHtml(value);
                return true;
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

            return false;
        }

        public static bool HexHasAlphaChannel(string value)
        {
            if (!TryExtractHexColorChannels(value, MinimumHexColorLength, out _, out _, out _, out var alpha))
            {
                return false;
            }

            return !string.IsNullOrEmpty(alpha);
        }

        public const int MinimumHexColorLength = 3;

        /// <summary>
        /// Tries to convert the hexadecimal <paramref name="value"/> to a color.
        /// </summary>
        /// <returns><c>true</c>, if parse hex color was tryed, <c>false</c> otherwise.</returns>
        /// <param name="value">Value.</param>
        /// <param name="color">Color.</param>
        public static bool TryParseHexColor(string value, out Color color, out bool hasAlpha)
        {
            return TryParseHexColor(value, MinimumHexColorLength, out color, out hasAlpha);
        }

        /// <summary>
        /// Tries to parse the hexadecimal <paramref name="value"/> into a color.
        /// </summary>
        /// <returns><c>true</c>, if parse hex color was tryed, <c>false</c> otherwise.</returns>
        /// <param name="value">Value.</param>
        /// <param name="minimumLength">Minimum length.</param>
        /// <param name="color">Color.</param>
        public static bool TryParseHexColor(string value, int minimumLength, out Color color, out bool hasAlpha)
        {
            hasAlpha = false;
            color = new Color();

            if (!TryExtractHexColorChannels(value, minimumLength, out var red, out var blue, out var green, out var alpha))
            {
                return false;
            }

            byte a = 255;

            if (!string.IsNullOrEmpty(alpha))
            {
                if (!byte.TryParse(alpha, NumberStyles.HexNumber, null as IFormatProvider, out a))
                {
                    return false;
                }
            }

            if (!byte.TryParse(red, NumberStyles.HexNumber, null as IFormatProvider, out var r))
            {
                return false;
            }

            if (!byte.TryParse(green, NumberStyles.HexNumber, null as IFormatProvider, out var g))
            {
                return false;
            }

            if (!byte.TryParse(blue, NumberStyles.HexNumber, null as IFormatProvider, out var b))
            {
                return false;
            }

            color = Color.FromArgb(a, r, g, b);

            return true;
        }

        public static bool TryExtractHexColorChannels(string value,
                                                      int minimumLength,
                                                      out string red,
                                                      out string blue,
                                                      out string green,
                                                      out string alpha)
        {
            alpha = string.Empty;
            green = string.Empty;
            blue = string.Empty;
            red = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();

            if (value.StartsWith("#", StringComparison.Ordinal))
            {
                value = value.Substring(1, value.Length - 1);
            }

            if (value.Length < minimumLength)
            {
                return false;
            }

            switch (value.Length)
            {
                case 3: // RGB
                    red = value.Substring(0, 1);
                    green = value.Substring(1, 1);
                    blue = value.Substring(2, 1);
                    break;
                case 4: // ARGB
                    alpha = value.Substring(0, 1);
                    red = value.Substring(1, 1);
                    green = value.Substring(2, 1);
                    blue = value.Substring(3, 1);
                    break;
                case 6: // RRGGBB
                    red = value.Substring(0, 2);
                    green = value.Substring(2, 2);
                    blue = value.Substring(4, 2);
                    break;
                case 8: // AARRGGB
                    alpha = value.Substring(0, 2);
                    red = value.Substring(2, 2);
                    green = value.Substring(4, 2);
                    blue = value.Substring(6, 2);
                    break;
                default:
                    return false;
            }

            alpha = alpha.ToLower();
            red = red.ToLower();
            green = green.ToLower();
            blue = blue.ToLower();

            return true;
        }

        /// <summary>
        /// Converts a hue value to an RGB formating
        /// </summary>
        /// <returns>The 2 rgb.</returns>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <param name="vH">V h.</param>
        public static double Hue_2_RGB(double v1, double v2, double vH)
        {
            if (vH < 0)
            {
                vH += 1;
            }

            if (vH > 1)
            {
                vH -= 1;
            }

            if ((6 * vH) < 1)
            {
                return (v1 + (v2 - v1) * 6 * vH);
            }

            if ((2 * vH) < 1)
            {
                return (v2);
            }

            if ((3 * vH) < 2)
            {
                return (v1 + (v2 - v1) * ((2 / 3) - vH) * 6);
            }

            return (v1);
        }

        public static bool GetMatchingColor(string value, out Color match)
        {
            if (!TryParseHexColor(value, out var color, out _))
            {
                return false;
            }

            var hasAlphaChannel = HexHasAlphaChannel(value);

            match = Color.Empty;

            foreach (var c in NamedColorMap)
            {
                var isMatch = false;
                if (Math.Abs(c.Value.R - color.R) < double.Epsilon
                    && Math.Abs(c.Value.G - color.G) < double.Epsilon
                    && Math.Abs(c.Value.B - color.B) < double.Epsilon)
                {
                    match = c.Value;
                    isMatch = true;
                }

                if (isMatch)
                {
                    if (hasAlphaChannel)
                    {
                        if (Math.Abs(0.0d - color.A) < double.Epsilon)
                        {
                            return true;
                        }
                    }
                    else if (match.Name != "Transparent")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetHexString(this Color color, bool includeHexNotation)
        {
            return GetHexString(color.R, color.G, color.B, color.A, includeHexNotation);
        }

        public static string GetHexString(byte r, byte g, byte b, bool includeHexNotation)
        {
            var value = r.ToString("X2") + g.ToString("X2") + b.ToString("X2");

            return (includeHexNotation ? "#" : "") + value;
        }

        public static string GetHexString(byte r, byte g, byte b, byte a, bool includeHexNotation)
        {
            var value = a.ToString("X2") + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");

            return (includeHexNotation ? "#" : "") + value;
        }

        public static string GetRGBAString(this Color color)
        {
            return color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString();
        }
    }
}
