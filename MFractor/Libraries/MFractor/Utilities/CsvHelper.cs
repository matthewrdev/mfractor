using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for working with comma separated values.
    /// </summary>
    public static class CSVHelper
    {
        /// <summary>
        /// Parse the given CSV <paramref name="value"/> into an integer array.
        /// </summary>
        /// <returns>The int csv.</returns>
        /// <param name="value">Value.</param>
        public static int[] ParseIntCSV(string value, char separator = ',')
        {
            if (string.IsNullOrEmpty(value))
            {
                return new int[0];
            }

            return value.Split(separator).Select(v => int.Parse(v.Trim())).ToArray();
        }

        /// <summary>
        /// Parse the given CSV <paramref name="value"/> into a string array.
        /// </summary>
        /// <returns>The string csv.</returns>
        /// <param name="value">Value.</param>
        public static string[] ParseStringCSV(string value, char separator = ',')
        {
            if (string.IsNullOrEmpty(value))
            {
                return new string[0];
            }

            return value.Split(separator);
        }

        /// <summary>
        /// Create a CSV from <paramref name="values"/>.
        /// </summary>
        /// <returns>The csv.</returns>
        /// <param name="values">Values.</param>
        public static string CreateCsv(IEnumerable<long> values, char separator = ',')
        {
            if (values == null || !values.Any())
            {
                return string.Empty;
            }

            return string.Join(separator.ToString(), values);
        }

        /// <summary>
        /// Create a CSV from <paramref name="values"/>.
        /// </summary>
        /// <returns>The csv.</returns>
        /// <param name="values">Values.</param>
        public static string CreateCsv(IEnumerable<int> values, char separator = ',')
        {
            if (values == null || !values.Any())
            {
                return string.Empty;
            }

            return string.Join(separator.ToString(), values);
        }

        /// <summary>
        /// Create a CSV from <paramref name="values"/>.
        /// </summary>
        /// <returns>The csv.</returns>
        /// <param name="values">Values.</param>
        public static string CreateCsv(IEnumerable<string> values, char separator = ',')
        {
            if (values == null || !values.Any())
            {
                return string.Empty;
            }

            return string.Join(separator.ToString(), values);
        }
    }
}
