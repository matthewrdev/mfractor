using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for computing a SHA1 hash for a string, stream or file input.
    /// </summary>
    public class SHA1Helper
    {
        /// <summary>
        /// Compute a SHA1 hash for string encoded as UTF8
        /// </summary>
        /// <param name="s">String to be hashed</param>
        /// <returns>40-character hex string</returns>
        public static string FromString(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);

            using (var sha1 = SHA1.Create())
            {
                var hashBytes = sha1.ComputeHash(bytes);

                return HexStringFromBytes(hashBytes);
            }
        }

        /// <summary>
        /// Compute a SHA1 hash for a <paramref name="stream"/>.
        /// </summary>
        /// <returns>The stream.</returns>
        /// <param name="stream">Data.</param>
        public static string FromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return FromString(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Compute a SHA1 hash for a <paramref name="filePath"/>.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filePath">File path.</param>
        public static string FromFile(string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                return FromStream(fs);
            }
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}
