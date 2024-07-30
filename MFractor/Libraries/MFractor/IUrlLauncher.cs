using System;
namespace MFractor
{
    /// <summary>
    /// A helper service for launching URLs from MFractor.
    /// <para/>
    /// The URL launcher will automatically add the 'utm_source' argument.
    /// <para/>
    /// An additional benefit of the URL launcher is verifying behaviour in unit tests.
    /// </summary>
    public interface IUrlLauncher
    {
        /// <summary>
        /// Opens the given <paramref name="url"/>.
        /// </summary>
        /// <param name="url"></param>
        void OpenUrl(string url, bool addUtmSource = true);

        /// <summary>
        /// Opens the given <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri"></param>
        void OpenUrl(Uri uri, bool addUtmSource = true);
    }
}
