using System;
using System.Globalization;

namespace MFractor.Fonts.Utilities
{
    public static class FontGlyphCodeHelper
    {
        /// <summary>
        /// Converts an escaped unicode character (eg: '&#xf84a;') to its single digit char character.
        /// <para/>
        /// If the <paramref name="value"/> is not an es
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EscapedUnicodeCharacterToGlyphCharacter(string value)
        {
            if (!TryEscapedUnicodeCharacterToGlyphCodePoint(value, out var codepoint))
            {
                return string.Empty;
            }

            try
            {
                var result = char.ToString((char)codepoint);

                return result;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Converts an escaped unicode character (eg: '&#xf84a;') to its single digit char character.
        /// <para/>
        /// If the <paramref name="value"/> is not an es
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryEscapedUnicodeCharacterToGlyphCodePoint(string value, out uint codepoint)
        {
            codepoint = 0;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.Length != 8)
            {
                return false;
            }

            if (!value.StartsWith("&#x") || !value.EndsWith(";"))
            {
                return false;
            }

            var newValue = value.Replace(";", string.Empty).Replace("&#x", string.Empty);

            if (!uint.TryParse(newValue, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var numeric))
            {
                return false;
            }

            codepoint = numeric;

            return true;
        }
    }
}