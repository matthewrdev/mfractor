using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for validating and converting string into C# symbol names. 
    /// </summary>
	public static class CSharpNameHelper
	{
        /// <summary>
        /// A regex that validates that the first char in a string is valid in a C# name.
        /// </summary>
		public static readonly Regex NameFirstPartRegex = new Regex("[@_a-zA-Z]", RegexOptions.Compiled);

        /// <summary>
        /// A regex that validates that a string is a valid c# name.
        /// </summary>
		public static readonly Regex NameSecondPartRegex = new Regex("^[a-zA-Z0-9_]*$", RegexOptions.Compiled);

        /// <summary>
        /// Checks if the <paramref name="name"/> is a valid C# symbol name.
        /// </summary>
        /// <returns><c>true</c>, if valid CS harp name was ised, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
		public static bool IsValidCSharpName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			var isValid = true;
			var value = name;
			var firstPart = value.Substring(0, 1);
			var secondPart = value.Substring(1, value.Length - 1);

			if (!NameFirstPartRegex.IsMatch(firstPart))
			{
				isValid = false;
			}
			else
			{
				isValid = NameSecondPartRegex.IsMatch(secondPart);
			}

			return isValid;
		}

        /// <summary>
        /// Converts the <paramref name="name"/> to a valid C# symbol name.
        /// </summary>
        /// <returns>The to valid CS harp name.</returns>
        /// <param name="name">Name.</param>
		public static string ConvertToValidCSharpName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return "";
			}

			var value = name;
			var firstPart = value.Substring(0, 1);
			var secondPart = value.Substring(1, value.Length - 1);

            var output = string.Join("", secondPart.Select(c => NameSecondPartRegex.IsMatch(c.ToString()) ? c.ToString() : ""));

            output = output.Replace("-", "");

			if (NameFirstPartRegex.IsMatch(firstPart))
            {
                output = firstPart + output;
			}

            return output;
		}

		/// <summary>
		/// Converts the <paramref name="name"/> to a valid C# symbol name.
		/// </summary>
		/// <returns>The to valid CS harp name.</returns>
		/// <param name="name">Name.</param>
		public static string ToDotNetName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return "";
			}

			var split = name.Split('-');

			var dotNetName = "";
			foreach (var s in split)
			{
				dotNetName += s.FirstCharToUpper();
			}

			return ConvertToValidCSharpName(dotNetName);
		}
	}
}

