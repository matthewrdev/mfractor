using System;
using System.Text;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for encoding and decoding base64 strings.
    /// </summary>
    public static class Base64Helper
    {
        /// <summary>
        /// Decode the <paramref name="value"/> from a Base64 string.
        /// </summary>
        /// <returns>The decode.</returns>
        /// <param name="value">The value.</param>
        public static string Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Encode the <paramref name="value"/> into a base64 string.
        /// </summary>
        /// <returns>The encode.</returns>
        /// <param name="value">Value.</param>
        public static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }
    }
}
