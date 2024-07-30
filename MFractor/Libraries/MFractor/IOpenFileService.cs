using System;
namespace MFractor
{
    /// <summary>
    /// Opens a file or directory in Finder (OSX) or Explorer (Windows).
    /// </summary>
    public interface IOpenFileInBrowserService
    {
        /// <summary>
        /// Opens and selects the given <paramref name="path"/>.
        /// </summary>
        /// <returns><c>true</c>, if and select was opened, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        bool OpenAndSelect(string path);
    }
}
