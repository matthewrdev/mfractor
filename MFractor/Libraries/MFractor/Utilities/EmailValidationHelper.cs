using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for validating emails.
    /// </summary>
    public class EmailValidationHelper
    {
        bool invalid = false;

        /// <summary>
        /// Is the provided <paramref name="email"/> valid?
        /// </summary>
        /// <returns><c>true</c>, if valid email was ised, <c>false</c> otherwise.</returns>
        /// <param name="email">Email.</param>
        public bool IsValidEmail(string email)
        {
            invalid = false;

            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", this.DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
            {
                return false;
            }

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Domains the mapper.
        /// </summary>
        /// <returns>The mapper.</returns>
        /// <param name="match">Match.</param>
        string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            var domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}