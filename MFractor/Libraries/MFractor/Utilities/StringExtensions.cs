using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace MFractor.Utilities
{
    /// <summary>
    /// A collection of helper methods for working with strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the first character in the string to its upper case variant.
        /// </summary>
        /// <returns>The char to upper.</returns>
        /// <param name="input">Input.</param>
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (input.Length == 1)
            {
                return input[0].ToString().ToUpper();
            }

            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// Converts the first character in the string to its lower case variant.
        /// </summary>
        /// <returns>The char to lower.</returns>
        /// <param name="input">Input.</param>
        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (input.Length == 1)
            {
                return input[0].ToString().ToLower();
            }

            return input[0].ToString().ToLower() + input.Substring(1);
        }

        /// <summary>
        /// Given the <paramref name="input"/>, locates the first character and surrounds it with <paramref name="left"/> and  <paramref name="right"/>.
        /// </summary>
        /// <returns>The char to lower.</returns>
        /// <param name="input">Input.</param>
        public static string SurroundFirstCharWith(this string input, string left, string right)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (input.Length == 1)
            {
                return left + input[0].ToString() + right;
            }

            return left + input[0].ToString() + right + input.Substring(1);
        }

        /// <summary>
        /// Finds all uppercase characters in the <paramref name="input"/> string and inserts a space between them.
        /// </summary>
        /// <returns>The upper letters by space.</returns>
        /// <param name="input">Input.</param>
        public static string SeparateUpperLettersBySpace(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var output = "";

            foreach (var s in input)
            {
                if (char.IsUpper(s) && !string.IsNullOrEmpty(output))
                {
                    output += " ";
                }
                output += s;
            }

            return output;
        }

        /// <summary>
        /// Converts the first character in each word of this string to its upper case variant.
        /// </summary>
        /// <returns>The char to upper.</returns>
        /// <param name="input">Input.</param>
        public static string WordStartingCharactersToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return string.Join(" ", input.Split(' ').Select(v => v.FirstCharToUpper()));
        }

        /// <summary>
        /// Converts the provided string to a stream.
        /// </summary>
        /// <returns>The stream.</returns>
        /// <param name="s">S.</param>
        public static Stream AsStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Reads a string from the provided stream.
        /// </summary>
        /// <returns>The stream.</returns>
        /// <param name="stream">Stream.</param>
        public static string AsString(this Stream stream)
        {
			stream.Position = 0;
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
        }

        /// <summary>
        /// Removes all diacritics (accent marks such as á) from the provided text, creating a plain ASCII string.
        /// </summary>
        /// <returns>The diacritics.</returns>
        /// <param name="text">Text.</param>
        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// The html to plain text regular expression.
        /// </summary>
        public const string HtmlToPlainTextRegularExpression = "<[^>]*>";

        /// <summary>
        /// The html to plain text regex, using <see cref="HtmlToPlainTextRegex"/>
        /// </summary>
        public static readonly Regex HtmlToPlainTextRegex = new Regex(HtmlToPlainTextRegularExpression);

        /// <summary>
        /// Converts the provided HTML content to a plain text string, removing all HTML tags.
        /// </summary>
        /// <returns>The to plain text.</returns>
        /// <param name="html">Html.</param>
        public static string HtmlToPlainText(this string html)
        {
            return HtmlToPlainTextRegex.Replace(html, string.Empty);
        }

        /// <summary>
        /// Removes all whitespace characters from the given <paramref name="string"/>.
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string RemoveWhitespace(this string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return @string;
            }

            return new string(@string.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        /// <summary>
        /// Escapes the HTML sensitive characters in the <paramref name="string"/> such as '"' or '&', converting them to their escaped representations.
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string EscapeHtml(this string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return @string;
            }

            return SecurityElement.Escape(@string);
        }

        /// <summary>
        /// Converts escaped HTML characters in the <paramref name="string"/> to their unescape representations.
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string UnescapeHtml(this string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return @string;
            }

            return @string.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
        }

        /// <summary>
        /// Gets all indexes of the <paramref name="value"/> in the provided <paramref name="input"/>/
        /// </summary>
        /// <param name="input"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<int> AllIndexesOf(this string input, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Enumerable.Empty<int>();
            }

            var indexes = new List<int>();
            for (var index = 0; ; index += value.Length)
            {
                index = input.IndexOf(value, index);
                if (index == -1)
                {
                    return indexes;
                }

                indexes.Add(index);
            }
        }

        /// <summary>
        /// Check if the current value is equals to at least one of the provided options.
        /// </summary>
        /// <param name="options">The options to check against the current instance.</param>
        /// <returns><c>true</c> if at least one of the values of the list matches.</returns>
        public static bool In(this string value, params string[] options) => options.Any(o => o == value);

        /// <summary>
        /// Check if the current value is not equals to all of the provided options.
        /// </summary>
        /// <param name="options">The options to check against the current instance.</param>
        /// <returns><c>true</c> if none of the options matches.</returns>
        public static bool NotIn(this string value, params string[] options) => !value.In(options);
    }
}
